using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        private SqlDataAdapter adapter;// { get; set; }
        public DataSet TablesWithData { get; set; }

        private List<string> tablesNames = new List<string>();

        //private DataRow dr;
        private string connectionString;

        public DbContext(string connectionString)
        {
            this.connectionString = connectionString;
            adapter = new SqlDataAdapter();
                //string sqlCommand = "MERGE ЗАПРОС";
                //adapter.InsertCommand = new SqlCommand(sqlCommand);
            TablesWithData = new DataSet();
            Preparing(new SqlConnection(connectionString)); //TODO: нужны ли мне все таблицы?
        }

        //public void GetTablesNames(SqlConnection connection)
        //{
        //    try
        //    {
        //        using (connection)
        //        {
        //            connection.Open();
        //            DataTable dbTables = connection.GetSchema("Tables");
        //            for (int i = 0; i < dbTables.Rows.Count; i++)
        //            {
        //                tablesNames.Add(dbTables.Rows[i].ItemArray[2]?.ToString());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

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
                        tablesNames.Add(tableName);
                        DataTable table = new DataTable(tableName);
                        table.Columns.AddRange(GetColumnsFromTable(connection, tableName));
                            //var sql = SqlGenerator.GetSelectAllString(tableName);
                            //adapter.SelectCommand = new SqlCommand(sql, connection);
                            //adapter.Fill(table);
                        TablesWithData.Tables.Add(table);
                    }
                    //    DataColumn cellColumn = TablesWithData.Tables["Cells"].Columns["ShipID"];
                    //cellColumn.AllowDBNull = true;

                    //DataColumn shipColumn = TablesWithData.Tables["Ships"].Columns["Id"];
                    //    shipColumn.Unique = true;

                    //DataRelation relation = new DataRelation("CellShip", cellColumn, shipColumn);
                    //TablesWithData.Relations.Add(relation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private DataColumn[] GetColumnsFromTable(SqlConnection connection, string tableName)
        {
            List<DataColumn> columns = new List<DataColumn>();
            var columnsTable = new DataTable();
            var sqlQuery = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'";
            adapter.SelectCommand = new SqlCommand(sqlQuery, connection);
            adapter.Fill(columnsTable);
            for (int j = 0; j < columnsTable.Rows.Count; j++)
            {
                columns.Add(new DataColumn(columnsTable.Rows[j].ItemArray[0].ToString()));
            }
            return columns.ToArray();
        }

        public DataTable GetTable(string tableName)
        {
            return TablesWithData.Tables[tableName];
        }
        public DataTable GetTableWithData(string tableName)
        {
            DataTable dataTable = new DataTable();
            SqlConnection connection = new SqlConnection(connectionString);
                using (connection)
                {
                    try
                    {
                        connection.Open();
                        string sqlQuery = $"SELECT * FROM {tableName}";
                        adapter.SelectCommand = new SqlCommand(sqlQuery, connection);
                        adapter.Fill(dataTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            return dataTable;
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