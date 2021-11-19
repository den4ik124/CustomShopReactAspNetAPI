using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORM_Repos_UoW
{
    public static partial class SqlGenerator
    {
        public static string GetSelectAllString(string table)
        {
            return $"SELECT * FROM {table}";
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