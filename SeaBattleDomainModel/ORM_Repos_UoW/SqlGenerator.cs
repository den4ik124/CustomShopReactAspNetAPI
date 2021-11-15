using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Repos_UoW
{
    public static class SqlGenerator
    {
        public static string GetSelectAllString(string table)
        {
            return $"SELECT * FROM {table}";
        }

        public static string GetInsertIntoString(Dictionary<string, object> columnsValues, string table)
        {
            StringBuilder insertIntoSql = new StringBuilder();
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();
            
            insertIntoSql.Append($"INSERT INTO {table} (");
            
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
    }
}
