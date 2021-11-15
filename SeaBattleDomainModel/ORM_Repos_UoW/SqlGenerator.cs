using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
