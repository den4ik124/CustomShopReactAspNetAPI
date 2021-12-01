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
        private Assembly assembly;
        private Stack<string> deleteSqlQueries;
        private Stack<string> updateSqlQueries;

        public SqlGenerator()
        {
            this.assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
            this.deleteSqlQueries = new Stack<string>();
            this.updateSqlQueries = new Stack<string>();
        }

        #region Methods.Private

        private string SelectJoinSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string whereFilterTable = "", int id = -1)
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
            selectQueryBuilder.Remove(selectQueryBuilder.Length - 2, 1);

            var relatedTable = tablePropetriesNames.First(type => type.Key
                                                                      .GetCustomAttribute<TableAttribute>()
                                                                      .IsRelatedTable).Key;
            string relatedTableName = relatedTable.GetCustomAttribute<TableAttribute>()
                                                  .TableName;
            var relatedTableProperties = relatedTable.GetProperties()
                                                     .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                 && prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign);

            selectQueryBuilder.Append($" FROM [{relatedTableName}]\n");

            var notRelatedTables = tablePropetriesNames.Where(i => i.Key.GetCustomAttribute<TableAttribute>().IsRelatedTable == false);

            foreach (var table in notRelatedTables)
            {
                var currentTable = table.Key.GetCustomAttribute<TableAttribute>();
                var currentTableName = currentTable.TableName;
                var primaryColumnName = table.Key.GetProperties()
                                                 .FirstOrDefault(p => p.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                      && p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                                 .GetCustomAttribute<ColumnAttribute>().ColumnName;

                var relatedPropertyColumn = relatedTableProperties.First(prop => prop.GetCustomAttribute<ColumnAttribute>().BaseType
                                                                                        .GetCustomAttribute<TableAttribute>().TableName == currentTableName)
                                                 .GetCustomAttribute<ColumnAttribute>().ColumnName;
                string joinResult = $"LEFT JOIN {currentTableName} ON [{currentTableName}].[{primaryColumnName}] = [{relatedTableName}].[{relatedPropertyColumn}]\n";
                selectQueryBuilder.Append(joinResult);
            }
            if (id > 0)
            {
                var type = this.assembly.GetTypes().First(t => t.GetCustomAttributes<TableAttribute>().Count() > 0
                                                    && t.GetCustomAttribute<TableAttribute>().TableName == whereFilterTable);
                var primaryKeyColumnName = type.GetProperties().First(p => p.GetCustomAttributes<ColumnAttribute>().Count() > 0
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

                        var genericChildTables = genericType.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
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

        private string SelectFromSingleTableSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string tableName, int id = -1)
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

            selectQueryBuilder.Remove(selectQueryBuilder.Length - 2, 1);

            selectQueryBuilder.Append($" FROM [{tableName}]\n");
            if (id > 0)
            {
                var primaryColumnName = type.GetProperties()
                                             .FirstOrDefault(atr => atr.GetCustomAttribute<ColumnAttribute>()
                                                                       .KeyType == KeyType.Primary).Name;
                selectQueryBuilder.Append($" WHERE [{tableName}].[{primaryColumnName}] = {id}");
            }
            return selectQueryBuilder.ToString();
        }

        private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = "";
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
                                    .Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                    .Select(attribute => attribute.GetCustomAttribute<ColumnAttribute>().ColumnName).ToList();
            if (type.IsAbstract)
            {
                var columnMatching = this.assembly.GetTypes()
                                            .Where(assemblyType => assemblyType.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
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
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && property.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                selectQueryWithConditionBuilder.Append($"[{tableName}].[{columnName}] = {columnValue} AND\n");
            }
            selectQueryWithConditionBuilder.Remove(selectQueryWithConditionBuilder.Length - 4, 3);

            return selectQueryWithConditionBuilder.ToString();
        }

        private string GetSqlIfExists<T>(T item)
        {
            var type = item.GetType();

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && property.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);
            var primaryKeyProperty = type.GetProperties().First(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                             && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetCustomAttribute<ColumnAttribute>().ColumnName;

            var selectQueryBuilder = new StringBuilder($"SELECT [{tableName}].[{primaryKeyProperty}] FROM [{tableName}] WHERE\n");
            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                selectQueryBuilder.Append($"[{tableName}].[{columnName}] = {columnValue} AND\n");
            }
            selectQueryBuilder.Remove(selectQueryBuilder.Length - 4, 3);

            return selectQueryBuilder.ToString();
        }

        private string SetNullOrDeleteForeignKey(string tableName, string columnName = "", object value = default)
        {
            var type = this.assembly.GetTypes()
                                .First(assemblyType => assemblyType.GetCustomAttributes<TableAttribute>().Count() > 0
                                                    && assemblyType.GetCustomAttribute<TableAttribute>().TableName == tableName);
            var isPropertyAllowNull = type.GetProperties()
                                            .First(property => property.GetCustomAttribute<ColumnAttribute>().ColumnName == columnName)
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
            if (item.GetType().GetCustomAttribute<TableAttribute>().IsStaticDataTable == true)
            {
                return string.Empty;
            }
            var type = item.GetType();
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = GetTypeProperties(item, type);

            var deleteQueryStringBuilder = new StringBuilder($"DELETE FROM [{tableName}] WHERE\n");

            var primaryKeyProperty = type.GetProperties().First(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary);
            var primaryKeyColumnName = primaryKeyProperty.GetCustomAttribute<ColumnAttribute>().ColumnName;
            var primaryKeyValue = primaryKeyProperty.GetValue(item);

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
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

            deleteQueryStringBuilder.Remove(deleteQueryStringBuilder.Length - 5, 5);
            return deleteQueryStringBuilder.ToString() + Environment.NewLine;
        }

        private string GetUpdateConcreteItemSqlQuery<T>(T item, string columnName = "", object value = default)
        {
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                && property.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);
            var primaryColumnName = type.GetProperties()
                                        .First(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                        .Name;
            var primaryColumnValue = type.GetProperties()
                                         .FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                         .GetValue(item);
            var propertyValue = new Dictionary<string, object>();
            var updateQueryBuider = new StringBuilder($"UPDATE [{tableName}]\nSET\n");
            foreach (var property in properties)
            {
                var currentColumnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
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
            updateQueryBuider.Remove(updateQueryBuider.Length - 2, 1);
            if (columnName == "")
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
            IEnumerable<PropertyInfo> properties;
            if ((int)type.GetProperties().FirstOrDefault(property => property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item) > 0)
            {
                properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            }
            else
            {
                properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
              && property.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);
            }

            return properties;
        }

        #endregion Methods.Private

        #region Methods.Public

        public string GetInsertIntoSqlQuery<T>(T item)
        {
            var type = typeof(T);

            var result = GetInsertConcreteItemSqlQuery(item) + Environment.NewLine;

            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childs.Count() == 0)
            {
                return result;
            }
            foreach (var child in childs)
            {
                if (child.GetValue(item) == null)
                {
                    continue;
                }
                dynamic childInstance = child.GetValue(item);
                if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection)
                {
                    if (childInstance.GetType().GetInterface("IDictionary") != null)
                    {
                        foreach (var key in childInstance.Keys)
                        {
                            result += GetInsertIntoSqlQuery(key) + Environment.NewLine;
                            result += GetInsertIntoSqlQuery(childInstance[key]) + Environment.NewLine;
                        }
                    }
                    else
                    {
                        foreach (var element in childInstance)
                        {
                            result += GetInsertIntoSqlQuery(element) + Environment.NewLine;
                        }
                    }
                }
                else
                {
                    result += GetInsertIntoSqlQuery(childInstance) + Environment.NewLine;
                }
            }
            return result;
        }

        public string GetInsertConcreteItemSqlQuery<T>(T item)
        {
            var type = item.GetType();
            var prefix = "";
            var postfix = "";

            if (type.GetCustomAttribute<TableAttribute>().IsStaticDataTable == true)
            {
                prefix = $"IF NOT EXISTS (\n{GetSqlIfNotExists(item)})\nBEGIN\n";
                postfix = $"\nEND\nELSE\nBEGIN\n{GetSqlIfExists(item)}\nEND";
            }

            string columnMatching = "";
            int typeId = 0;
            var propertyValue = new Dictionary<string, object>();
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && property.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);

            var insertQueryBuilder = new StringBuilder($"INSERT INTO [{tableName}]\n");
            var columnNameStringBuilder = new StringBuilder();
            var columnValueStringBuilder = new StringBuilder();
            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);

                columnNameStringBuilder.Append($"[{tableName}].[{columnName}],");
                if (columnValue == null)
                {
                    columnValueStringBuilder.Append("NULL,");
                }
                else
                {
                    columnValueStringBuilder.Append($"{columnValue},");
                }
            }
            if (type.GetCustomAttribute<InheritanceRelationAttribute>() != null)
            {
                columnMatching = type.GetCustomAttribute<InheritanceRelationAttribute>().ColumnMatching;
                typeId = type.GetCustomAttribute<TypeAttribute>().TypeID;
                columnNameStringBuilder.Append($"[{tableName}].[{columnMatching}],");
                columnValueStringBuilder.Append($"{typeId},");
            }

            columnNameStringBuilder.Remove(columnNameStringBuilder.Length - 1, 1);
            columnValueStringBuilder.Remove(columnValueStringBuilder.Length - 1, 1);

            insertQueryBuilder.Append($"({columnNameStringBuilder}) VALUES ({columnValueStringBuilder})");
            insertQueryBuilder.Append("\nSELECT CAST(scope_identity() AS int)");

            return prefix + insertQueryBuilder.ToString() + postfix;
        }

        public string GetSelectJoinString(Type type, int id = -1)
        {
            var tablePropetriesNames = new Dictionary<Type, List<string>>();

            string tableName;
            List<string> propertiesNames;
            DefineTableAndProperties(type, out tableName, out propertiesNames);

            tablePropetriesNames.Add(type, propertiesNames);

            var childTables = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childTables.Count() == 0)
            {
                if (id > -1)
                {
                    return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName, id);
                }
                else
                {
                    return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName);
                }
            }

            DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);

            if (id > 0)
            {
                string whereFilterTable = type.GetCustomAttribute<TableAttribute>().TableName;
                return SelectJoinSqlQuery(tablePropetriesNames, whereFilterTable, id);
            }

            return SelectJoinSqlQuery(tablePropetriesNames);
        }

        public string SelectFromSingleTableSqlQuery(Type type)
        {
            var selectQueryBuider = new StringBuilder("SELECT \n");

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            var properties = type.GetProperties().Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                .Select(property => property.GetCustomAttribute<ColumnAttribute>().ColumnName);
            foreach (var property in properties)
            {
                if (property.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{property}] AS [{tableName}{property}";
                    selectQueryBuider.Append($"[{tableName}].[{propAs}],\n");
                }
                else
                {
                    selectQueryBuider.Append($"[{tableName}].[{property}],\n");
                }
            }

            selectQueryBuider.Remove(selectQueryBuider.Length - 2, 1);

            selectQueryBuider.Append($" FROM [{tableName}]\n");
            return selectQueryBuider.ToString();
        }

        public IEnumerable<Type> GetDependentTypes(Type deletedType)
        {
            var types = this.assembly.GetTypes();
            var usefulTypes = types.Where(type => type.GetProperties()
                                                .Where(property => property.GetCustomAttributes<ColumnAttribute>()
                                                .Count() > 0)
                                    .Count() > 0);
            return usefulTypes.Where(type => type.GetProperties()
                                            .Where(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == deletedType)
                                            .Count() > 0);
        }

        public string GetUpdateSqlQuery<T>(T item, string columnName = "", object value = default)
        {
            if (this.updateSqlQueries == null)
            {
                this.updateSqlQueries = new Stack<string>();
            }
            this.updateSqlQueries.Push(GetUpdateConcreteItemSqlQuery<T>(item, columnName, value));

            return GetStringFromStack(this.updateSqlQueries);
        }

        public string GetDeleteSqlQuery(string tableName, int id)
        {
            if (this.deleteSqlQueries == null)
            {
                this.deleteSqlQueries = new Stack<string>();
            }

            var type = this.assembly.GetTypes().First(assemblyType => assemblyType.GetCustomAttributes<TableAttribute>().Count() > 0
                                                   && assemblyType.GetCustomAttribute<TableAttribute>().TableName == tableName);
            var primaryKeyColumnName = type.GetProperties()
                                            .First(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                            .GetCustomAttribute<ColumnAttribute>().ColumnName;

            if (type.GetCustomAttribute<TableAttribute>().IsRelatedTable)
            {
                return $"DELETE [{tableName}] WHERE [{tableName}].[{primaryKeyColumnName}] = {id}";
            }
            else if (type.GetCustomAttribute<TableAttribute>().IsStaticDataTable)
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
                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>().TableName;
                var foreignKeyColumnName = relatedType.GetProperties()
                                                      .First(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                        && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign
                                                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == type)
                                                      .GetCustomAttribute<ColumnAttribute>().ColumnName;
                var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, id);

                this.deleteSqlQueries.Push(updateQuery);
            }
            return GetStringFromStack(this.deleteSqlQueries);
        }

        public string GetDeleteSqlQuery<T>(string columnName, object value)
        {
            if (this.deleteSqlQueries == null)
            {
                this.deleteSqlQueries = new Stack<string>();
            }
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            this.deleteSqlQueries.Push($"DELETE [{tableName}] WHERE [{tableName}].[{columnName}] = {value}\n");

            var relatedTypes = GetDependentTypes(type);
            foreach (var relatedType in relatedTypes)
            {
                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>().TableName;
                var foreignKeyColumnName = relatedType.GetProperties()
                                                      .First(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                        && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign
                                                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == type)
                                                      .GetCustomAttribute<ColumnAttribute>().ColumnName;
                var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, value);

                this.deleteSqlQueries.Push(updateQuery);
            }
            return GetStringFromStack(this.deleteSqlQueries);
        }

        public string GetDeleteSqlQuery<T>(T item)
        {
            if (item == null)
            {
                return Environment.NewLine;
            }
            var type = item.GetType();

            if (this.deleteSqlQueries == null)
            {
                this.deleteSqlQueries = new Stack<string>();
            }

            this.deleteSqlQueries.Push(GetDeleteConcreteItemSqlQuery(item));

            int primaryKeyValue = (int)type.GetProperties().FirstOrDefault(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                              && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary)
                                            .GetValue(item);
            IEnumerable<Type> relatedTypes;
            if (type.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0 && !type.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass)
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
                var relatedTablename = relatedType.GetCustomAttribute<TableAttribute>().TableName;

                var foreignKeyColumnName = relatedType.GetProperties()
                                                      .First(property => property.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                        && property.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign
                                                                        && property.GetCustomAttribute<ColumnAttribute>().BaseType == type)
                                                      .GetCustomAttribute<ColumnAttribute>().ColumnName;

                var updateQuery = SetNullOrDeleteForeignKey(relatedTablename, foreignKeyColumnName, primaryKeyValue);
                this.deleteSqlQueries.Push(updateQuery);
            }

            var childs = type.GetProperties().Where(property => property.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

            if (childs.Count() > 0)
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
                        if (child.PropertyType.GetInterface("IDictionary") != null)
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
    }
}