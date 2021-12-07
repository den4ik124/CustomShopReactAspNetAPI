using OrmRepositoryUnitOfWork.Attributes;
using OrmRepositoryUnitOfWork.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrmRepositoryUnitOfWork
{
    public class SqlGenerator
    {
        private const int PropertyEndOfStringOffset = 2;
        private const int PropertyAmountOfDeletingSymbols = 1;
        private const int ConditionEndOfStringOffset = 4;
        private const int ConditionAmountOfDeletingSymbols = 3;

        private const string ScopeIdentity = "\nSELECT CAST(scope_identity() AS int)";

        private Assembly assembly;
        private Stack<string> deleteSqlQueries;
        private Stack<string> updateSqlQueries;
        private AttributeChecker attributeChecker;

        public SqlGenerator()
        {
            this.assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetCustomAttributes<DomainModelAttribute>().Any());
            if (this.assembly == null)
            {
                throw new ArgumentNullException($"The [{nameof(DomainModelAttribute)}] was not set to your assembly");
            }
            this.deleteSqlQueries = new Stack<string>();
            this.updateSqlQueries = new Stack<string>();
            this.attributeChecker = new AttributeChecker();
        }

        #region Methods.Public

        public string GetInsertIntoSqlQuery<T>(T item)
        {
            var type = typeof(T);
            var insertQueryBuidler = new StringBuilder(GetInsertConcreteItemSqlQuery(item) + Environment.NewLine);

            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (!childs.Any())
            {
                return insertQueryBuidler.ToString();
            }
            foreach (var child in childs)
            {
                if (child.GetValue(item) == null)
                {
                    continue;
                }
                dynamic childInstance = child.GetValue(item);

                var childAttribute = child.GetCustomAttribute<RelatedEntityAttribute>();
                if (childAttribute != null && childAttribute.IsCollection)
                {
                    if (childInstance.GetType().GetInterface(nameof(IDictionary<object, object>)) != null)
                    {
                        foreach (var key in childInstance.Keys)
                        {
                            insertQueryBuidler.Append(GetInsertIntoSqlQuery(key) + Environment.NewLine);
                            insertQueryBuidler.Append(GetInsertIntoSqlQuery(childInstance[key]) + Environment.NewLine);
                        }
                    }
                    else
                    {
                        foreach (var element in childInstance)
                        {
                            insertQueryBuidler.Append(GetInsertIntoSqlQuery(element) + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    insertQueryBuidler.Append(GetInsertIntoSqlQuery(childInstance) + Environment.NewLine);
                }
            }
            return insertQueryBuidler.ToString();
        }

        public string GetInsertConcreteItemSqlQuery<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Item was null");
            }
            var propertyValue = new Dictionary<string, object>();
            var columnNameStringBuilder = new StringBuilder();
            var columnValueStringBuilder = new StringBuilder();
            var columnMatching = string.Empty;
            var typeId = default(int);

            var type = item.GetType();

            var prefix = $"IF NOT EXISTS (\n{GetSqlIfNotExists(item)})\nBEGIN\n";
            var postfix = $"\nEND\nELSE\nBEGIN\n{GetSqlIfExists(item)}\nEND";

            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (typeTableAttribute == null)
            {
                throw new ArgumentNullException($"[{nameof(TableAttribute)} was not set for {type.Name} type");
            }

            var tableName = type.GetCustomAttribute<TableAttribute>()?.TableName;
            var propertiesWithColumnAttribute = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());
            if (!propertiesWithColumnAttribute.Any())
            {
                throw new Exception($"There are no properties with [{nameof(ColumnAttribute)}]");
            }
            var properties = propertiesWithColumnAttribute.Where(property => property.GetCustomAttribute<ColumnAttribute>()?.KeyType != KeyType.Primary);

            var insertQueryBuilder = new StringBuilder($"INSERT INTO [{tableName}]\n");
            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
                var columnValue = property.GetValue(item);

                columnNameStringBuilder.Append($"[{tableName}].[{columnName}], ");
                if (columnValue == null)
                {
                    columnValueStringBuilder.Append("NULL, ");
                }
                else
                {
                    columnValueStringBuilder.Append($"{columnValue}, ");
                }
            }
            var typeInheritanceAttribute = type.GetCustomAttribute<InheritanceRelationAttribute>();
            var typeTypeAttribure = type.GetCustomAttribute<TypeAttribute>();

            if (typeInheritanceAttribute != null && typeTypeAttribure != null)
            {
                columnNameStringBuilder.Append($"[{tableName}].[{typeInheritanceAttribute.ColumnMatching}], ");
                columnValueStringBuilder.Append($"{typeTypeAttribure.TypeID}, ");
            }

            columnNameStringBuilder.Remove(columnNameStringBuilder.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);
            columnValueStringBuilder.Remove(columnValueStringBuilder.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);

            insertQueryBuilder.Append($"({columnNameStringBuilder}) VALUES ({columnValueStringBuilder})");
            insertQueryBuilder.Append(ScopeIdentity);

            return prefix + insertQueryBuilder.ToString() + postfix;
        }

        public string GetSelectJoinString(Type type, int id = default)
        {
            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (typeTableAttribute == null)
            {
                throw new Exception($"Check [TableAttribute] on {type.Name}");
            }
            string tableName;
            List<string> propertiesNames;
            var tablePropetriesNames = new Dictionary<Type, List<string>>();
            DefineTableAndProperties(type, out tableName, out propertiesNames);

            tablePropetriesNames.Add(type, propertiesNames);

            var childTables = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Any());
            if (!childTables.Any())
            {
                if (id > default(int))
                {
                    return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName, id);
                }
                else
                {
                    return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName);
                }
            }

            DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);

            if (id > default(int))
            {
                string whereFilterTable = typeTableAttribute.TableName;
                return SelectJoinSqlQuery(tablePropetriesNames, whereFilterTable, id);
            }

            return SelectJoinSqlQuery(tablePropetriesNames);
        }

        public string SelectFromSingleTableSqlQuery(Type type)
        {
            var selectQueryBuider = new StringBuilder("SELECT \n");
            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (typeTableAttribute == null)
            {
                throw new Exception($"Check [{nameof(TableAttribute)}] on {type.Name}");
            }
            var tableName = typeTableAttribute.TableName;

            var propertiesWithColumnAttribute = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());
            if (!propertiesWithColumnAttribute.Any())
            {
                throw new Exception($"Any property of {type.Name} was not marked by [{nameof(ColumnAttribute)}]");
            }
            var columnsNames = propertiesWithColumnAttribute.Select(property => property.GetCustomAttribute<ColumnAttribute>()?.ColumnName);
            foreach (var column in columnsNames)
            {
                if (!String.IsNullOrEmpty(column) && column.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{column}] AS [{tableName}{column}";
                    selectQueryBuider.Append($"[{tableName}].[{propAs}],\n");
                }
                else
                {
                    selectQueryBuider.Append($"[{tableName}].[{column}],\n");
                }
            }

            selectQueryBuider.Remove(selectQueryBuider.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);

            selectQueryBuider.Append($" FROM [{tableName}]\n");
            return selectQueryBuider.ToString();
        }

        public string GetSelectIdForItem(Type type, string columnName, object value)
        {
            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (typeTableAttribute == null)
            {
                throw new Exception($"Type {type.Name} was not marked by [{nameof(TableAttribute)}]");
            }
            var tableName = typeTableAttribute.TableName;

            var propertiesWithColumnAttribute = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());
            if (!propertiesWithColumnAttribute.Any())
            {
                throw new Exception($"There are no any property marked by [{nameof(ColumnAttribute)}]");
            }
            var primaryKeyColumnName = GetPrimaryKeyProperty(type)?.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
            return $"SELECT [{tableName}].[{primaryKeyColumnName}] FROM Ships WHERE [{tableName}].[{columnName}] = {value}";
        }

        public IEnumerable<Type> GetDependentTypes(Type deletedType)
        {
            var types = this.assembly.GetTypes();
            var usefulTypes = types.Where(type => type.GetProperties()
                                                .Where(property => property.GetCustomAttributes<ColumnAttribute>().Any())
                                    .Any());
            var usefullTypesWithColumnAttributeProperties = usefulTypes.Where(type => type.GetProperties()
                                                                                          .Where(property => property.GetCustomAttributes<ColumnAttribute>().Any())
                                                                        .Any());
            if (!usefullTypesWithColumnAttributeProperties.Any())
            {
                throw new Exception($"There are no any property marked by [{nameof(ColumnAttribute)}]");
            }

            return usefulTypes.Where(type => type.GetProperties()
                                            .Where(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == deletedType)
                                            .Any());
        }

        public string GetUpdateSqlQuery<T>(T item, string columnName = "", object? value = default)
        {
            this.updateSqlQueries.Push(GetUpdateConcreteItemSqlQuery<T>(item, columnName, value));
            return GetStringFromStack(this.updateSqlQueries);
        }

        public string GetDeleteSqlQuery(string tableName, int id)
        {
            var typesWithTableAttribute = this.assembly.GetTypes().Where(assemblyType => assemblyType.GetCustomAttributes<TableAttribute>().Any());

            if (!typesWithTableAttribute.Any())
            {
                throw new Exception($"There is no any type marked with [{nameof(TableAttribute)}]");
            }

            var type = typesWithTableAttribute.First(assemblyType => assemblyType.GetCustomAttribute<TableAttribute>()?.TableName == tableName);
            var primaryKeyColumnName = GetPrimaryKeyProperty(type)?.GetCustomAttribute<ColumnAttribute>()?.ColumnName;

            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();

            if (typeTableAttribute != null && typeTableAttribute.IsRelatedTable)
            {
                return $"DELETE [{tableName}] WHERE [{tableName}].[{primaryKeyColumnName}] = {id}";
            }
            else if (typeTableAttribute != null && typeTableAttribute.IsStaticDataTable)
            {
                return string.Empty;
            }
            else
            {
                this.deleteSqlQueries.Push($"DELETE [{tableName}] WHERE [{tableName}].[{primaryKeyColumnName}] = {id}\n");
            }

            var relatedTypes = GetDependentTypes(type);
            foreach (var relatedType in relatedTypes)
            {
                this.attributeChecker.CheckTableAttribute(relatedType);
                this.attributeChecker.CheckColumnAttribute(relatedType);

                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>()?.TableName;

                var propertiesWithColumnAttribute = relatedType.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());
                if (!propertiesWithColumnAttribute.Any())
                {
                    throw new Exception($"There are no any property marked by [{nameof(ColumnAttribute)} inside {relatedType.Name} type]");
                }

                var foreignKeyColumnName = propertiesWithColumnAttribute.FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                                    && property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Foreign
                                                                    && property.GetCustomAttribute<ColumnAttribute>()?.BaseType == type)?
                                                  .GetCustomAttribute<ColumnAttribute>()?.ColumnName;
                if (foreignKeyColumnName == null)
                {
                    throw new Exception($"There are no any property inside {relatedType.Name} marked by [{nameof(ColumnAttribute)}].{nameof(KeyType)} or [{nameof(ColumnAttribute)}].BaseType");
                }
                var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, id);

                this.deleteSqlQueries.Push(updateQuery);
            }
            return GetStringFromStack(this.deleteSqlQueries);
        }

        public string GetDeleteSqlQuery<T>(string columnName, object value, IEnumerable<int> primaryKeysValues)
        {
            var type = typeof(T);

            this.attributeChecker.CheckTableAttribute(type);

            var tableName = type.GetCustomAttribute<TableAttribute>()?.TableName;
            this.deleteSqlQueries.Push($"DELETE [{tableName}] WHERE [{tableName}].[{columnName}] = {value}\n");

            var relatedTypes = GetDependentTypes(type);
            foreach (var relatedType in relatedTypes)
            {
                this.attributeChecker.CheckTableAttribute(relatedType);
                this.attributeChecker.CheckColumnAttribute(relatedType);

                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>()?.TableName;

                var foreignKeyColumnName = relatedType.GetProperties()
                                                      .First(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                                        && property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Foreign
                                                                        && property.GetCustomAttribute<ColumnAttribute>()?.BaseType == type)?
                                                      .GetCustomAttribute<ColumnAttribute>()?.ColumnName;
                foreach (var primaryKeyValue in primaryKeysValues)
                {
                    var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, primaryKeyValue);
                    this.deleteSqlQueries.Push(updateQuery);
                }
            }
            return GetStringFromStack(this.deleteSqlQueries);
        }

        public string GetDeleteSqlQuery<T>(T item)
        {
            if (item == null)
            {
                return Environment.NewLine;
            }
            this.deleteSqlQueries.Push(GetDeleteConcreteItemSqlQuery(item));

            var type = item.GetType();

            this.attributeChecker.CheckTableAttribute(type);
            this.attributeChecker.CheckColumnAttribute(type);

            var primaryKeyValue = (int?)GetPrimaryKeyProperty(type)?.GetValue(item);
            IEnumerable<Type> relatedTypes;
            if (type.GetCustomAttributes<InheritanceRelationAttribute>().Any() && !type.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass)
            {
                type = type.BaseType;
            }
            if (type.GetCustomAttribute<TableAttribute>().IsStaticDataTable)
            {
                return string.Empty;
            }
            relatedTypes = GetDependentTypes(type);

            foreach (var relatedType in relatedTypes)
            {
                this.attributeChecker.CheckTableAttribute(relatedType);
                this.attributeChecker.CheckColumnAttribute(relatedType);
                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>()?.TableName;

                var foreignKeyColumnName = relatedType.GetProperties()
                                                      .First(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                                        && property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Foreign
                                                                        && property.GetCustomAttribute<ColumnAttribute>()?.BaseType == type)?
                                                      .GetCustomAttribute<ColumnAttribute>()?.ColumnName;

                var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, primaryKeyValue);
                this.deleteSqlQueries.Push(updateQuery);
            }

            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Any());

            if (childs.Any())
            {
                foreach (var child in childs)
                {
                    if (child.GetValue(item) == default)
                    {
                        continue;
                    }
                    dynamic propertyObject = child.GetValue(item);
                    if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                    {
                        if (child.PropertyType.GetInterface(nameof(IDictionary<object, object>)) != null)
                        {
                            var keys = propertyObject.Keys;
                            foreach (var key in keys)
                            {
                                this.deleteSqlQueries.Push(GetDeleteSqlQuery(key));
                                this.deleteSqlQueries.Push(GetDeleteSqlQuery(propertyObject[key]));
                            }
                        }
                        else
                        {
                            foreach (var collectionItem in propertyObject)
                            {
                                this.deleteSqlQueries.Push(GetDeleteSqlQuery(collectionItem));
                            }
                        }
                    }
                    else
                    {
                        this.deleteSqlQueries.Push(GetDeleteSqlQuery(propertyObject));
                    }
                }
            }

            return GetStringFromStack(this.deleteSqlQueries);
        }

        #endregion Methods.Public

        #region Methods.Private

        private PropertyInfo GetPrimaryKeyProperty(Type type)
        {
            var properties = type.GetProperties();
            var primaryKeyProperty = properties.FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Primary);

            if (primaryKeyProperty == null)
            {
                throw new Exception($"[{nameof(ColumnAttribute)}].{nameof(KeyType)} was not set to the primary key property");
            }
            return primaryKeyProperty;
        }

        private string SelectJoinSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string whereFilterTable = "", int id = default)
        {
            var selectQueryBuilder = new StringBuilder("SELECT\n");
            foreach (var table in tablePropetriesNames)
            {
                string currentTableName = table.Key.GetCustomAttribute<TableAttribute>().TableName;
                foreach (var property in tablePropetriesNames[table.Key])
                {
                    if (property.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        selectQueryBuilder.Append($"[{currentTableName}].[{property}] AS [{currentTableName}{property}],\n");
                        continue;
                    }
                    selectQueryBuilder.Append($"[{currentTableName}].[{property}],\n");
                }
            }
            selectQueryBuilder.Remove(selectQueryBuilder.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);

            var relatedTable = tablePropetriesNames.First(type => type.Key
                                                                      .GetCustomAttribute<TableAttribute>()
                                                                      .IsRelatedTable).Key;
            string relatedTableName = relatedTable.GetCustomAttribute<TableAttribute>()
                                                  .TableName;
            var relatedTableProperties = relatedTable.GetProperties()
                                                     .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Any()
                                                                 && prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign);

            selectQueryBuilder.Append($" FROM [{relatedTableName}]\n");

            var notRelatedTables = tablePropetriesNames.Where(i => i.Key.GetCustomAttribute<TableAttribute>().IsRelatedTable == false);

            foreach (var table in notRelatedTables)
            {
                var currentTable = table.Key.GetCustomAttribute<TableAttribute>();
                var currentTableName = currentTable.TableName;
                var primaryColumnName = GetPrimaryKeyProperty(table.Key)?.GetCustomAttribute<ColumnAttribute>()?.ColumnName;

                var relatedPropertyColumn = relatedTableProperties.First(prop => prop.GetCustomAttribute<ColumnAttribute>().BaseType
                                                                                        .GetCustomAttribute<TableAttribute>().TableName == currentTableName)
                                                 .GetCustomAttribute<ColumnAttribute>().ColumnName;
                string joinResult = $"LEFT JOIN {currentTableName} ON [{currentTableName}].[{primaryColumnName}] = [{relatedTableName}].[{relatedPropertyColumn}]\n";
                selectQueryBuilder.Append(joinResult);
            }
            if (id > 0)
            {
                var type = this.assembly.GetTypes().First(t => t.GetCustomAttributes<TableAttribute>().Any()
                                                    && t.GetCustomAttribute<TableAttribute>().TableName == whereFilterTable);
                var primaryKeyColumnName = type.GetProperties().First(p => p.GetCustomAttributes<ColumnAttribute>().Any()
                                                    && p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetCustomAttribute<ColumnAttribute>().ColumnName;

                string whereResult = $"WHERE [{whereFilterTable}].[{primaryKeyColumnName}] = {id}";
                selectQueryBuilder.Append(whereResult);
            }
            return selectQueryBuilder.ToString();
        }

        private void DefineRelatedEntities(ref Dictionary<Type, List<string>> tablePropetriesNames, ref string tableName, ref List<string> propertiesNames, IEnumerable<PropertyInfo> childTables)
        {
            foreach (var childTable in childTables)
            {
                if (childTable.PropertyType.IsGenericType)
                {
                    var genericTypes = childTable.PropertyType.GetGenericArguments();
                    foreach (var genericType in genericTypes)
                    {
                        DefineTableAndProperties(genericType, out tableName, out propertiesNames);
                        tablePropetriesNames.Add(genericType, propertiesNames);

                        var genericChildTables = genericType.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Any());
                        DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, genericChildTables);
                    }
                    continue;
                }
                DefineTableAndProperties(childTable.PropertyType, out tableName, out propertiesNames);
                if (!tablePropetriesNames.ContainsKey(childTable.PropertyType))
                {
                    tablePropetriesNames.Add(childTable.PropertyType, propertiesNames);
                }
            }
        }

        private string SelectFromSingleTableSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string tableName, int id = default)
        {
            var selectQueryBuilder = new StringBuilder("SELECT \n");

            var type = tablePropetriesNames.First(keyType => keyType.Key.GetCustomAttribute<TableAttribute>().TableName == tableName).Key;

            var properties = tablePropetriesNames[type];
            foreach (var property in properties)
            {
                if (property.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{property}] AS [{tableName}{property}";
                    selectQueryBuilder.Append($"[{tableName}].[{propAs}],\n");
                }
                else
                {
                    selectQueryBuilder.Append($"[{tableName}].[{property}],\n");
                }
            }

            selectQueryBuilder.Remove(selectQueryBuilder.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);

            selectQueryBuilder.Append($" FROM [{tableName}]\n");
            if (id > 0)
            {
                var primaryColumnName = GetPrimaryKeyProperty(type).Name;
                selectQueryBuilder.Append($" WHERE [{tableName}].[{primaryColumnName}] = {id}");
            }
            return selectQueryBuilder.ToString();
        }

        private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = string.Empty;
            propertiesNames = new List<string>();

            if (type.GetCustomAttribute<RelatedEntityAttribute>() != null && type.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
            {
                if (type.IsGenericType)
                {
                    var genericTypes = type.GetGenericArguments();
                    foreach (var genericType in genericTypes)
                    {
                        GetSinglePropertyData(genericType, out tableName, out propertiesNames);
                    }
                }
            }
            else
            {
                GetSinglePropertyData(type, out tableName, out propertiesNames);
            }
        }

        private void GetSinglePropertyData(Type type, out string tableName, out List<string> propertiesNames)
        {
            var typeTableAttribute = type.GetCustomAttribute<TableAttribute>();
            tableName = typeTableAttribute.TableName;
            propertiesNames = type.GetProperties()
                                    .Where(property => property.GetCustomAttributes<ColumnAttribute>().Any())
                                    .Select(attribute => attribute.GetCustomAttribute<ColumnAttribute>().ColumnName).ToList();
            if (type.IsAbstract)
            {
                var columnMatching = this.assembly.GetTypes()
                                            .Where(assemblyType => assemblyType.GetCustomAttributes<InheritanceRelationAttribute>().Any()
                                            && assemblyType.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false)
                                            .Select(a => a.GetCustomAttribute<InheritanceRelationAttribute>().ColumnMatching)
                                            .FirstOrDefault();
                string propertyName = $"{columnMatching}";
                propertiesNames.Add(propertyName);
            }
        }

        private string GetStringFromStack(Stack<string> stack)
        {
            var stackStringBuilder = new StringBuilder();
            do
            {
                stackStringBuilder.Append(stack.Pop());
            } while (stack.Count > 0);

            return stackStringBuilder.ToString();
        }

        private string GetSqlIfNotExists<T>(T item)
        {
            var type = item.GetType();
            var selectQueryWithConditionBuilder = new StringBuilder($"{GetSelectJoinString(type)} \n WHERE ");

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                                    && property.GetCustomAttribute<ColumnAttribute>().IsUniq);

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                if (columnValue == null)
                {
                    selectQueryWithConditionBuilder.Append($"[{tableName}].[{columnName}] IS NULL AND\n");
                }
                else
                {
                    selectQueryWithConditionBuilder.Append($"[{tableName}].[{columnName}] = {columnValue} AND\n");
                }
            }
            selectQueryWithConditionBuilder.Remove(selectQueryWithConditionBuilder.Length - ConditionEndOfStringOffset, ConditionAmountOfDeletingSymbols);

            return selectQueryWithConditionBuilder.ToString();
        }

        private string GetSqlIfExists<T>(T item)
        {
            var type = item.GetType();

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                        && property.GetCustomAttribute<ColumnAttribute>().IsUniq);

            var primaryKeyProperty = type.GetProperties().First(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                             && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetCustomAttribute<ColumnAttribute>().ColumnName;

            var selectQueryBuilder = new StringBuilder($"SELECT [{tableName}].[{primaryKeyProperty}] FROM [{tableName}] WHERE\n");
            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                if (columnValue == null)
                {
                    selectQueryBuilder.Append($"[{tableName}].[{columnName}] IS NULL AND\n");
                }
                else
                {
                    selectQueryBuilder.Append($"[{tableName}].[{columnName}] = {columnValue} AND\n");
                }
            }
            selectQueryBuilder.Remove(selectQueryBuilder.Length - ConditionEndOfStringOffset, ConditionAmountOfDeletingSymbols);

            return selectQueryBuilder.ToString();
        }

        private string SetNullOrDeleteForeignKey(string? tableName, string? columnName = "", object? value = default)
        {
            var type = this.assembly.GetTypes()
                                .First(assemblyType => assemblyType.GetCustomAttributes<TableAttribute>().Any()
                                                    && assemblyType.GetCustomAttribute<TableAttribute>()?.TableName == tableName);
            this.attributeChecker.CheckColumnAttribute(type);

            var isPropertyAllowNull = type.GetProperties()
                                            .First(property => property.GetCustomAttribute<ColumnAttribute>()?.ColumnName == columnName)
                                            .GetCustomAttribute<ColumnAttribute>().AllowNull;
            if (isPropertyAllowNull)
            {
                return $"UPDATE [{tableName}]\nSET [{tableName}].[{columnName}] = NULL\nWHERE [{tableName}].[{columnName}] = {value}\n";
            }
            else
            {
                return $"DELETE FROM [{tableName}]\nWHERE [{tableName}].[{columnName}] = {value}\n";
            }
        }

        private string GetDeleteConcreteItemSqlQuery<T>(T item)
        {
            var type = item.GetType();
            this.attributeChecker.CheckTableAttribute(type);
            this.attributeChecker.CheckColumnAttribute(type);

            if (type.GetCustomAttribute<TableAttribute>()?.IsStaticDataTable == true)
            {
                return string.Empty;
            }
            var tableName = type.GetCustomAttribute<TableAttribute>()?.TableName;
            var properties = GetTypeProperties(item, type);

            var deleteQueryStringBuilder = new StringBuilder($"DELETE FROM [{tableName}] WHERE\n");

            var primaryKeyProperty = type.GetProperties().First(property => property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Primary);
            var primaryKeyColumnName = primaryKeyProperty.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
            var primaryKeyValue = primaryKeyProperty.GetValue(item);

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
                var value = property.GetValue(item);
                if (value == null)
                {
                    deleteQueryStringBuilder.Append($"[{tableName}].[{columnName}] IS NULL AND\n");
                }
                else
                {
                    deleteQueryStringBuilder.Append($"[{tableName}].[{columnName}] = {value} AND\n");
                }
            }

            deleteQueryStringBuilder.Remove(deleteQueryStringBuilder.Length - ConditionEndOfStringOffset, ConditionAmountOfDeletingSymbols);
            return deleteQueryStringBuilder.ToString() + Environment.NewLine;
        }

        private string GetUpdateConcreteItemSqlQuery<T>(T item, string columnName = "", object? value = default)
        {
            var type = typeof(T);

            this.attributeChecker.CheckTableAttribute(type);
            this.attributeChecker.CheckColumnAttribute(type);

            var tableName = type.GetCustomAttribute<TableAttribute>()?.TableName;
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any()
                                                && property.GetCustomAttribute<ColumnAttribute>()?.KeyType != KeyType.Primary);
            var primaryKeyProperty = GetPrimaryKeyProperty(type);
            var primaryColumnName = primaryKeyProperty.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
            var primaryColumnValue = primaryKeyProperty.GetValue(item);
            var propertyValue = new Dictionary<string, object>();
            var updateQueryBuider = new StringBuilder($"UPDATE [{tableName}]\nSET\n");
            foreach (var property in properties)
            {
                var currentColumnName = property.GetCustomAttribute<ColumnAttribute>()?.ColumnName;
                var columnValue = property.GetValue(item);
                if (columnValue == null)
                {
                    updateQueryBuider.Append($"[{tableName}].[{currentColumnName}] = NULL,\n");
                }
                else
                {
                    updateQueryBuider.Append($"[{tableName}].[{currentColumnName}] = {columnValue},\n");
                }
            }
            updateQueryBuider.Remove(updateQueryBuider.Length - PropertyEndOfStringOffset, PropertyAmountOfDeletingSymbols);
            if (columnName == string.Empty)
            {
                updateQueryBuider.Append($"WHERE [{tableName}].[{primaryColumnName}] = {primaryColumnValue}");
            }
            else
            {
                updateQueryBuider.Append($"WHERE [{tableName}].[{columnName}] = {value}");
            }
            return updateQueryBuider.ToString();
        }

        /// <summary>
        /// Selection properties with or without primary key property
        /// </summary>
        /// <typeparam name="T">type of item</typeparam>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<PropertyInfo> GetTypeProperties<T>(T item, Type type)
        {
            this.attributeChecker.CheckColumnAttribute(type);
            IEnumerable<PropertyInfo> properties;
            if ((int?)type.GetProperties()?.FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>()?.KeyType == KeyType.Primary)?.GetValue(item) > 0)
            {
                properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any());
            }
            else
            {
                properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Any()
              && property.GetCustomAttribute<ColumnAttribute>()?.KeyType != KeyType.Primary);
            }

            return properties;
        }

        #endregion Methods.Private
    }
}