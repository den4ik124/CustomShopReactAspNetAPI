using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ORM_Repos_UoW
{
    public class SqlGenerator
    {
        private Assembly assembly;

        private Stack<string> deleteSqlQueries { get; set; }
        private Stack<string> updateSqlQueries { get; set; }

        public SqlGenerator()
        {
            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
        }

        #region Methods.Private

        private string SelectJoinSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string whereFilterTable = "", int id = -1)
        {
            #region SELECT .. JOIN Example

            //SELECT
            //  [table1].[id] AS [table1Id],
            //  [table1].[prop2],
            //  [table1].[...],

            //  [table2].[Id] AS [table2Id],
            //  [table2].[prop2],
            //  [table2].[...],

            //  [table3].[Id] AS [table3Id],
            //  [table3].[prop2],
            //  ... . ...
            //  [tableN].[Id] AS [tableNId],
            //  [tableN].[prop2]
            // FROM [table1]
            // LEFT JOIN [table2] ON [table2].[Id] = [relTable].[FK_table2_Id]
            // LEFT JOIN [table3] ON [table3].[Id] = [relTable].[FK_table3_Id]
            // ...
            // LEFT JOIN [tableN] ON [tableN].[Id] = [relTable].[FK_tableN_Id]

            #endregion SELECT .. JOIN Example

            StringBuilder sb = new StringBuilder("SELECT\n");
            foreach (var table in tablePropetriesNames)
            {
                string currentTableName = table.Key.GetCustomAttribute<TableAttribute>().TableName;
                foreach (var property in tablePropetriesNames[table.Key])
                {
                    if (property.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append($"[{currentTableName}].[{property}] AS [{currentTableName}{property}],\n");
                        continue;
                    }
                    sb.Append($"[{currentTableName}].[{property}],\n");
                }
            }
            sb.Remove(sb.Length - 2, 1);

            var relatedTable = tablePropetriesNames.First(type => type.Key
                                                                      .GetCustomAttribute<TableAttribute>()
                                                                      .IsRelatedTable).Key;
            string relatedTableName = relatedTable.GetCustomAttribute<TableAttribute>()
                                                  .TableName;
            var relatedTableProperties = relatedTable.GetProperties()
                                                     .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                 && prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Foreign);

            sb.Append($" FROM [{relatedTableName}]\n");

            var notRelatedTables = tablePropetriesNames.Where(i => i.Key.GetCustomAttribute<TableAttribute>().IsRelatedTable == false);

            foreach (var table in notRelatedTables)
            {
                var currentTable = table.Key.GetCustomAttribute<TableAttribute>();
                var currentTableName = currentTable.TableName;
                var primaryColumnName = table.Key.GetProperties().FirstOrDefault(p => p.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                                          && p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).Name;

                var relatedPropertyColumn = relatedTableProperties.First(prop => prop.GetCustomAttribute<ColumnAttribute>()
                                                                                        .BaseType
                                                                                        .GetCustomAttribute<TableAttribute>()
                                                                                        .TableName == currentTableName).Name;
                string joinResult = $"LEFT JOIN {currentTableName} ON [{currentTableName}].[{primaryColumnName}] = [{relatedTableName}].[{relatedPropertyColumn}]\n";
                sb.Append(joinResult);
            }
            if (id > 0)
            {
                string whereResult = $"WHERE [{whereFilterTable}].[Id] = {id}"; //TODO : избавиться от "Id"
                sb.Append(whereResult);
            }
            return sb.ToString();
        }

        private void DefineRelatedEntities(ref Dictionary<Type, List<string>> tablePropetriesNames, ref string tableName, ref List<string> propertiesNames, IEnumerable<PropertyInfo> childTables)
        {
            childTables.OrderBy(i => i.PropertyType.GetCustomAttribute<TableAttribute>().IsRelatedTable); //TODO: выяснить нужна ли эта сортировка или можно придумать что получше?
                                                                                                          //TODO продумать как прикрутить рекурсию для доступа к свойствам Cell

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
            StringBuilder sb = new StringBuilder("SELECT \n");

            var type = tablePropetriesNames.First(t => t.Key.GetCustomAttribute<TableAttribute>().TableName == tableName).Key;

            var properties = tablePropetriesNames[type];// type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                if (prop.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{prop}] AS [{tableName}{prop}";
                    sb.Append($"[{tableName}].[{propAs}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
                else
                {
                    sb.Append($"[{tableName}].[{prop}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
            }

            sb.Remove(sb.Length - 2, 1);

            sb.Append($" FROM [{tableName}]\n");
            if (id > 0)
            {
                var primaryColumnName = type.GetProperties()
                                             .FirstOrDefault(atr => atr.GetCustomAttribute<ColumnAttribute>()
                                                                       .KeyType == KeyType.Primary).Name;
                sb.Append($" WHERE [{tableName}].[{primaryColumnName}] = {id}");
            }
            return sb.ToString();
        }

        public string SelectFromSingleTableSqlQuery(Type type, int id = -1)
        {
            StringBuilder sb = new StringBuilder("SELECT \n");

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                .Select(prop => prop.GetCustomAttribute<ColumnAttribute>().ColumnName); //tablePropetriesNames[type];// type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            foreach (var prop in properties)
            {
                if (prop.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    string propAs = $"{prop}] AS [{tableName}{prop}";
                    sb.Append($"[{tableName}].[{propAs}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
                else
                {
                    sb.Append($"[{tableName}].[{prop}],\n");//.GetCustomAttribute<ColumnAttribute>().ColumnName
                }
            }

            sb.Remove(sb.Length - 2, 1);

            sb.Append($" FROM [{tableName}]\n");
            //if (id > 0)
            //{
            //    var primaryColumnName = type.GetProperties()
            //                                 .FirstOrDefault(atr => atr.GetCustomAttribute<ColumnAttribute>()
            //                                                           .KeyType == KeyType.Primary).Name;
            //    sb.Append($" WHERE [{tableName}].[{primaryColumnName}] = {id}");
            //}
            return sb.ToString();
        }

        private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = "";
            propertiesNames = new List<string>();

            //TODO: подумать, как работать с типом Dictionary здесь!!! Вылетает исключение

            var test = type.GetCustomAttribute<RelatedEntityAttribute>();
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
            var attribute = type.GetCustomAttribute<TableAttribute>();
            tableName = attribute.TableName;
            if (attribute.IsRelatedTable)
            {
                //   tableName += $".rel";
            }
            propertiesNames = type.GetProperties()
                                    .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                    .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName).ToList();
            if (type.IsAbstract)
            {
                var columnMatching = assembly.GetTypes()
                                            .Where(t => t.GetCustomAttributes<InheritanceRelationAttribute>().Count() > 0
                                            && t.GetCustomAttribute<InheritanceRelationAttribute>().IsBaseClass == false)
                                            .Select(a => a.GetCustomAttribute<InheritanceRelationAttribute>().ColumnMatching)
                                            .First();
                //string propertyName = $"{columnMatching} AS  {tableName}{columnMatching}";
                string propertyName = $"{columnMatching}";
                propertiesNames.Add(propertyName);
            }
        }

        private static void SetForeignKeyNull(IEnumerable<Type> types, dynamic item)
        {
            var sqlGenerator = new SqlGenerator();
            foreach (var dependentType in types)
            {
                sqlGenerator.GetUpdateSqlQuery(dependentType, item); //TODO: подумать в какой Stack запихивать этот запрос
            }
        }

        private static IEnumerable<Type> GetDependentTypes(Type deletedType)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
            var types = assembly.GetTypes();
            var usefulTypes = types.Where(t => t.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0).Count() > 0);
            return usefulTypes.Where(t => t.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                        && prop.GetCustomAttribute<ColumnAttribute>().BaseType == deletedType).Count() > 0);
            //return AppDomain.CurrentDomain.GetAssemblies()
            //                                .First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0)
            //                                    .GetTypes()
            //                                    .Where(t => t.GetProperties()
            //                                        .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
            //                                    .Count() > 0)
            //                                .Where(t => t.GetProperties()
            //                                    .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
            //                                                && prop.GetCustomAttribute<ColumnAttribute>().BaseType == deletedType).Count() > 0);
        }

        #endregion Methods.Private

        #region Methods.Public

        public string GetInsertIntoSqlQuery<T>(T item)
        {
            //TODO: Resolve conflict on home laptop
            var type = typeof(T);

            string result = GetInsertConcreteItemSqlQuery(item) + Environment.NewLine;
            var childs = type.GetProperties().Where(p => p.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
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
                //Dictionary<object,object> childInstance = (Dictionary<object, object>)child.GetValue(item); //получение значения свойства
                dynamic childInstance = child.GetValue(item); //получение значения свойства
                if (child.GetCustomAttribute<RelatedEntityAttribute>().IsCollection) //если свойство - коллекция, то для каждого элемента нужно получить имя колонки и значение
                {
                    if (childInstance.GetType().GetInterface("IDictionary") != null)
                    {
                        foreach (var element in childInstance)
                        {
                            result += GetInsertIntoSqlQuery(element.Key) + Environment.NewLine;
                            result += GetInsertIntoSqlQuery(element.Value) + Environment.NewLine;
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
                    result += GetInsertIntoSqlQuery(childInstance) + Environment.NewLine; //TODO: тут креш когда прилетатет NULL
                }
            }
            return result;
        }

        //IF NOT EXISTS
        //     (SELECT * FROM [Points]
        //     WHERE[Points].[X] = 99
        //                AND[Points].[Y] = 99)
        //BEGIN
        //     INSERT INTO[Points]
        //     ([Points].[X],[Points].[Y]) VALUES(99,99);
        //END

        private string GetInsertConcreteItemSqlQuery<T>(T item)
        {
            var type = item.GetType();
            string prefix = "";
            string postfix = "";

            if (type.GetCustomAttribute<TableAttribute>().IsStaticDataTable == true)
            {
                prefix = $"IF NOT EXISTS (\n{GetSqlIfNotExists(item)})\nBEGIN\n";
                postfix = "\nEND";
            }
            string columnMatching = "";
            int typeId = 0;
            var propertyValue = new Dictionary<string, object>();
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && prop.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);

            var sb = new StringBuilder($"INSERT INTO [{tableName}]\n");
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
            sb.Append($"({columnNameStringBuilder}) VALUES ({columnValueStringBuilder})");

            var test = prefix + sb.ToString() + postfix;
            return prefix + sb.ToString() + postfix;
        }

        private string GetSqlIfNotExists<T>(T item)
        {
            var type = typeof(T);
            StringBuilder sb = new StringBuilder($"{GetSelectJoinString(type)} \n WHERE ");

            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                            && prop.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                sb.Append($"[{tableName}].[{columnName}] = {columnValue} AND\n");
            }
            sb.Remove(sb.Length - 4, 3);

            return sb.ToString();
        }

        public string GetSelectAllString(string table)
        {
            return $"SELECT * FROM {table}";
        }

        //SELECT
        // BattleFields.Id AS BF_ID,
        // BattleFields.SideLength,
        // Cells.Id AS CELL_ID,
        // Cells.ShipId,
        // Cells.BattleFieldID,
        // Ships.Id AS SHIP_ID,
        // Ships.TypeId,
        // Ships.Velocity,
        // Ships.[Range],
        // Ships.Size,
        // ShipTypes.[Type]
        //FROM BattleFields
        // LEFT JOIN Cells ON Cells.BattleFieldID = BattleFields.Id
        // LEFT JOIN ships on Ships.id = cells.shipid
        // LEFT JOIN ShipTypes on Ships.TypeId = ShipTypes.Id
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

        public string GetUpdateSqlQuery(Type type, dynamic item)
        {
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            Type itemType = item.GetType();
            var id = itemType.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
            var columnName = itemType.GetProperties().First(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).Name;

            var test = $"UPDATE [{tableName}] SET\n[{tableName}].[{columnName}] = NULL\nWHERE [{tableName}].[{columnName}] = {id}";
            //UPDATE Cells SET
            //Cells.ShipID = NULL
            //WHERE Cells.ShipID = 24

            return "";
        }

        public string GetUpdateSqlQuery<T>(T item, string columnName = "", object value = default)
        {
            updateSqlQueries.Push(GetUpdateConcreteItemSqlQuery<T>(item, columnName, value));

            return GetStringFromStack(updateSqlQueries);
        }

        private string GetStringFromStack(Stack<string> stack)
        {
            var stackStringBuilder = new StringBuilder();
            do
            {
                stackStringBuilder.Append(stack.Pop()/* + Environment.NewLine*/);
            } while (stack.Count > 0);

            return stackStringBuilder.ToString();
        }

        private string GetUpdateConcreteItemSqlQuery<T>(T item, string columnName = "", object value = default)
        {
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
                                                && prop.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);
            var primaryColumnName = type.GetProperties().First(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).Name;
            var primaryColumnValue = type.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item);
            var propertyValue = new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder($"UPDATE [{tableName}]\nSET\n");
            foreach (var property in properties)
            {
                var currentColumnName = property.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var columnValue = property.GetValue(item);
                sb.Append($"[{tableName}].[{currentColumnName}] = {columnValue},\n");
            }
            sb.Remove(sb.Length - 2, 1);
            if (columnName == "")
            {
                sb.Append($"WHERE [{tableName}].[{primaryColumnName}] = {primaryColumnValue}");
            }
            else
            {
                sb.Append($"WHERE [{tableName}].[{columnName}] = {value}");
            }
            //UPDATE [tableName]
            //SET
            //[prop1] = [item.value1],
            //[prop2] = [item.value2],
            //...
            //[propN] = [item.valueN]
            //WHERE [tableName].[Id] = item.Id

            return sb.ToString();
        }

        //DELETE {tableName} WHERE {parameter} = {value} [AND {parameter2} = {value}]
        public string GetDeleteSqlQuery(string tableName, int id)
        {
            //TODO: продумать удаление связанных сущностей
            return $"DELETE [{tableName}] WHERE [{tableName}].[Id] = {id}"; //TODO: подумать как убрать "id" из строки
        }

        public string GetDeleteSqlQuery<T>(string columnName, object value)
        {
            throw new NotImplementedException();
            //1. получаем тип, где [Table(TableName)] = tableName;
            //2. создаем запрос за удаление
            //3. проходим рекурсивно по вложенным сущностям
            //4. создаем запрос за удаление

            //SetForeignKeyNull();

            var tableName = typeof(T).GetCustomAttribute<TableAttribute>().TableName;

            return $"DELETE [{tableName}] WHERE [{tableName}].[{columnName}] = {value}"; //TODO: подумать как убрать "id" из строки
        }

        public string GetDeleteSqlQuery<T>(T item)
        {
            if (item == null)
            {
                return Environment.NewLine;
            }
            var type = item.GetType();

            if (deleteSqlQueries == null)
            {
                deleteSqlQueries = new Stack<string>();
            }

            deleteSqlQueries.Push(GetDeleteConcreteItemSqlQuery(item)); //TODO: подумать как удалить строку из БД по всем свойствам ITEM-а

            var childs = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);

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
                                deleteSqlQueries.Push(GetDeleteSqlQuery(key));
                                deleteSqlQueries.Push(GetDeleteSqlQuery(propertyObject[key]));
                            }
                        }
                        else
                        {
                            foreach (var collectionItem in propertyObject)
                            {
                                deleteSqlQueries.Push(GetDeleteSqlQuery(collectionItem));
                            }
                        }
                    }
                    else
                    {
                        deleteSqlQueries.Push(GetDeleteSqlQuery(propertyObject));
                    }
                }
            }

            return GetStringFromStack(deleteSqlQueries);
        }

        private string GetDeleteConcreteItemSqlQuery<T>(T item)
        {
            if (item.GetType().GetCustomAttribute<TableAttribute>().IsStaticDataTable == true)
            {
                return string.Empty;
            }
            var type = item.GetType();
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;
            IEnumerable<PropertyInfo> properties = GetTypeProperties(item, type);

            var deleteQueryStringBuilder = new StringBuilder($"DELETE [{tableName}] WHERE\n");
            foreach (var prop in properties)
            {
                var columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                var value = prop.GetValue(item);
                if (value == null)
                {
                    value = "NULL";
                }
                deleteQueryStringBuilder.Append($"[{tableName}].[{columnName}] = {value} AND\n");
            }
            Debug.WriteLine(deleteQueryStringBuilder);
            deleteQueryStringBuilder.Remove(deleteQueryStringBuilder.Length - 5, 5);
            Debug.WriteLine(deleteQueryStringBuilder);

            return deleteQueryStringBuilder.ToString() + Environment.NewLine;
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties<T>(T item, Type type)
        {
            IEnumerable<PropertyInfo> properties;
            if ((int)type.GetProperties().FirstOrDefault(prop => prop.GetCustomAttribute<ColumnAttribute>().KeyType == KeyType.Primary).GetValue(item) > 0)
            {
                properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);
            }
            else
            {
                properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0
              && prop.GetCustomAttribute<ColumnAttribute>().KeyType != KeyType.Primary);
            }

            return properties;
        }

        //MERGE {TargetTableName} AS TargetTable
        //Using {SourceTableName} AS Source
        //ON TargetTable.Id = Source.Id
        //WHEN MATCHED THEN UPDATE SET
        //    TargetTable.{Field1} = Source.{Field1},
        //    TargetTable.{Field2} = Source.{Field2},
        //    TargetTable.{Field3} = Source.{Field3},
        //      .....................................................
        //    TargetTable.{FieldN} = Source.{FieldN},
        //WHEN NOT MATCHED THEN
        //      INSERT({Field1}, {Field2}, ... , {FieldN})
        //      VALUES(Source.{Field1}, Source.{Field2}, ... , Source.{FieldN});

        public string GetMergeString(string TargetTableName, string SourceTableName, List<string> columns)
        {
            StringBuilder mergeSQL = new StringBuilder(@$"MERGE {TargetTableName} AS TargetTable Using {SourceTableName} AS Source ON TargetTable.Id = Source.Id WHEN MATCHED THEN UPDATE SET ");
            StringBuilder updateColumns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (var column in columns)
            {
                updateColumns.Append($"TargetTable.{column} =  Source.{column},");
            }
            updateColumns.Remove(updateColumns.Length - 1, 1);

            mergeSQL.Append(updateColumns.ToString() + "\nWHEN NOT MATCHED THEN\nINSERT (");
            StringBuilder insertColumns = new StringBuilder();

            foreach (var column in columns)
            {
                insertColumns.Append($"{column}, ");
                values.Append($"Source.{column}, ");
            }
            insertColumns.Remove(insertColumns.Length - 2, 2);
            values.Remove(values.Length - 2, 2);

            mergeSQL.Append(insertColumns.ToString() + ") VALUES (");
            mergeSQL.Append(values.ToString() + ");");
            return mergeSQL.ToString();
        }

        #endregion Methods.Public
    }
}