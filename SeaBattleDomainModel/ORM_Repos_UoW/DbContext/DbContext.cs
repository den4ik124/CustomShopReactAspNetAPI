using System;
using System.Data;
using System.Data.SqlClient;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        private SqlDataAdapter adapter;// { get; set; }
        public DataSet TablesWithData { get; set; }

        //private DataRow dr;
        private string connectionString;

        public DbContext(string connectionString)
        {
            this.connectionString = connectionString;
            adapter = new SqlDataAdapter();
            TablesWithData = new DataSet();
            Preparing(new SqlConnection(connectionString)); //TODO: нужны ли мне все таблицы?
        }

        private void Preparing(SqlConnection connection)
        {
            try
            {
                using (connection)
                {
                    connection.Open();
                    DataTable dbTables = connection.GetSchema("Tables");
                    for (int i = 0; i < dbTables.Rows.Count; i++)
                    {
                        var tableName = dbTables.Rows[i].ItemArray[2]?.ToString();
                        var sql = SqlGenerator.GetSelectAllString(tableName);
                        DataTable table = new DataTable(tableName);
                        adapter.SelectCommand = new SqlCommand(sql, connection);
                        adapter.Fill(table);
                        TablesWithData.Tables.Add(table);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public DataTable GetTable(string tableName)
        {
            var tables = TablesWithData.Tables;
            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].TableName == tableName)
                {
                    return tables[i];
                }
            }
            throw new NullReferenceException();
            return null;
        }

        public void SaveChanges()
        {
            //int res = 0;
            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                for (int i = 0; i < TablesWithData.Tables.Count; i++)
                {
                    adapter.SelectCommand = new SqlCommand($"SELECT * FROM {TablesWithData.Tables[i].TableName}", sqlConnection);
                    SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
                    /*res += */
                    adapter.Update(TablesWithData, TablesWithData.Tables[i].TableName);
                }
            }
            //return res;
        }

        private void Add(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }

        private void Update(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }

        private void Delete(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }

        //private string? GetTableName(System.Reflection.CustomAttributeData attributes) //TODO: исправить на private вне тестов
        //{
        //    List<string> tablesNames = new List<string>();
        //    for (int i = 0; i < this.TablesWithData.Tables.Count; i++)
        //    {
        //        tablesNames.Add(this.TablesWithData.Tables[i].TableName);
        //    }
        //    var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value?.ToString() == arg);
        //    return tableName;
        //}
    }
}