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

        public SqlGenerator()
        {
            assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetCustomAttributes<DomainModelAttribute>().Count() > 0);
        }

        public static string GetSelectAllString(string table)
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
        public string GetSelectJoinString(Type type)
        {
            //var tablePropetriesNames = new Dictionary<string, List<string>>();
            var tablePropetriesNames = new Dictionary<Type, List<string>>();

            string tableName;
            List<string> propertiesNames;
            DefineTableAndProperties(type, out tableName, out propertiesNames);

            //tablePropetriesNames.Add(tableName, propertiesNames);
            tablePropetriesNames.Add(type, propertiesNames);

            var childTables = type.GetProperties().Where(prop => prop.GetCustomAttributes<RelatedEntityAttribute>().Count() > 0);
            if (childTables.Count() == 0)
            {
                return SelectFromSingleTableSqlQuery(tablePropetriesNames, tableName);
            }

            DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);

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
            Debug.WriteLine(sb.ToString());
            var res = sb.ToString();

            sb.Remove(sb.Length - 2, 1);

            Debug.WriteLine(sb.ToString());

            string relatedTableName = tablePropetriesNames.First(type => type.Key.GetCustomAttribute<TableAttribute>().IsRelatedTable).Key.GetCustomAttribute<TableAttribute>().TableName;
            sb.Append($" FROM [{relatedTableName}]\n");

            Debug.WriteLine(sb.ToString());
            //TODO: дописать JOIN-ы в запрос

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
            // LEFT JOIN [table2] ON [table2].[Id] = [table1].[FK_table2_Id]
            // LEFT JOIN [table3] ON [table3].[Id] = [table1].[FK_table3_Id]
            // ...
            // LEFT JOIN [tableN] ON [tableN].[Id] = [table1].[FK_tableN_Id]

            //var columns =
            return sb.ToString();
        }

        private void DefineRelatedEntities(ref Dictionary<Type, List<string>> tablePropetriesNames, ref string tableName, ref List<string> propertiesNames, IEnumerable<PropertyInfo> childTables)
        {
            childTables.OrderBy(i => i.PropertyType.GetCustomAttribute<TableAttribute>().IsRelatedTable); //TODO: выяснить нужна ли эта сортировка или можно придумать что получше?
            //TODO продумать как прикрутить рекурсию для доступа к свойствам Cell
            //if (childTables.Count() == 0)
            //{
            //    return;
            //}

            //DefineRelatedEntities(ref tablePropetriesNames, ref tableName, ref propertiesNames, childTables);
            foreach (var childTable in childTables)
            {
                //DefineTableAndProperties(childTable.PropertyType, out tableName, out propertiesNames);
                var test = childTable.GetCustomAttribute<RelatedEntityAttribute>().IsCollection;
                if (childTable.PropertyType.IsGenericType)
                {
                    var genericTypes = childTable.PropertyType.GetGenericArguments();
                    foreach (var genericType in genericTypes)
                    {
                        DefineTableAndProperties(genericType, out tableName, out propertiesNames);
                        tablePropetriesNames.Add(genericType, propertiesNames);
                    }
                }
            }
        }

        //private string SelectFromSingleTableSqlQuery(Dictionary<string, List<string>> tablePropetriesNames, string tableName)
        private string SelectFromSingleTableSqlQuery(Dictionary<Type, List<string>> tablePropetriesNames, string tableName)
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
            sb.Remove(sb.Length - 1, 1);
            sb.Append($" FROM [{tableName}];");
            Debug.WriteLine(sb.ToString());

            return sb.ToString();
        }

        private void DefineTableAndProperties(Type type, out string tableName, out List<string> propertiesNames)
        {
            tableName = "";
            propertiesNames = new List<string>();

            //TODO: подумать, как работать с типом List здесь!!! Вылетает исключение

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
                tableName += $".rel";
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

        public static string GetInsertIntoString(Dictionary<string, object> columnsValues, string table)
        {
            StringBuilder insertIntoSql = new StringBuilder($"INSERT INTO {table} (");
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (var columnValue in columnsValues)
            {
                columns.Append(columnValue.Key + ", ");
                values.Append(columnValue.Value + ", ");
            }
            columns.Remove(columns.Length - 2, 2);
            values.Remove(values.Length - 2, 2);

            insertIntoSql.Append(columns.ToString() + ") VALUES (" + values.ToString() + ")");
            return insertIntoSql.ToString();
        }

        //DELETE {tableName} WHERE {parameter} = {value} [AND {parameter2} = {value}]
        public static string GetDeleteString(string tableName, Dictionary<string, object> columnsValues, ConditionStatement conditionStatement)
        {
            StringBuilder deleteSQL = new StringBuilder($"DELETE {tableName} WHERE ");
            deleteSQL.Append($"{columnsValues.First().Key} = {columnsValues.First().Value}");
            columnsValues.Remove(columnsValues.First().Key);
            if (columnsValues.Count > 1)
            {
                string conditionText;
                switch (conditionStatement)
                {
                    case ConditionStatement.AND:
                        conditionText = " AND ";
                        break;

                    case ConditionStatement.OR:
                        conditionText = " OR ";
                        break;

                    default:
                        conditionText = " ";
                        break;
                }
                foreach (var columnValue in columnsValues)
                {
                    deleteSQL.Append($"{conditionText}{columnValue.Key} = {columnValue.Value}");
                }
                return deleteSQL.ToString();
            }
            else
            {
                return deleteSQL.ToString();
            }
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

        public static string GetMergeString(string TargetTableName, string SourceTableName, List<string> columns)
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
    }
}