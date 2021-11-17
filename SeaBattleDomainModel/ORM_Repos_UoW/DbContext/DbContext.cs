using SeaBattleDomainModel.DerivedShips;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        private SqlDataAdapter adapter;// { get; set; }
        public  DataSet TablesWithData { get; set; }
        //private DataRow dr;

        public DbContext(string connection)
        {
            sqlConnection = new SqlConnection(connection);
            adapter = new SqlDataAdapter();
            TablesWithData = new DataSet();

            Preparing(sqlConnection); //TODO: нужны ли мне все таблицы?
        }

        public void Create<T>(T item)
        {

        }

        private string? GetTableName(System.Reflection.CustomAttributeData attributes) //TODO: исправить на private вне тестов
        {
            List<string> tablesNames = new List<string>();
            for (int i = 0; i < this.TablesWithData.Tables.Count; i++)
            {
                tablesNames.Add(this.TablesWithData.Tables[i].TableName);
            }
            var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value.ToString() == arg);
            return tableName;
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
                        var tableName = dbTables.Rows[i].ItemArray[2];
                        var sql = $"SELECT * FROM {tableName}";
                        DataTable table = new DataTable(tableName.ToString());
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

        public int SaveChanges()
        {
            using (sqlConnection)
            {
                sqlConnection.Open();
                adapter.Update(TablesWithData);

                //Add(sqlConnection);
                //Update(sqlConnection);
                //Delete(sqlConnection);
            }
            throw new NotImplementedException();
        }

        private void Add(SqlConnection sqlConnection)
        {

        }

        private void Update(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }

        private void Delete(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }
    }
}