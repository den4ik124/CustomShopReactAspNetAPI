using ORM_Repos_UoW.DataMapper;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        private SqlDataAdapter adapter;
        public DataSet TablesWithData { get; set; }

        private Dictionary<Type, object> mappers;

        private List<string> tablesNames = new List<string>();

        private string connectionString;

        public DbContext(string connectionString)
        {
            this.connectionString = connectionString;
            this.adapter = new SqlDataAdapter();
            //string sqlCommand = "MERGE ЗАПРОС";
            //adapter.InsertCommand = new SqlCommand(sqlCommand);
            this.TablesWithData = new DataSet();
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
                        tablesNames.Add(tableName);
                        DataTable table = new DataTable(tableName);
                        table.Columns.AddRange(GetColumnsFromTable(connection, tableName));
                        table.PrimaryKey = new DataColumn[] { table.Columns["Id"] };

                        TablesWithData.Tables.Add(table);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public IDataMapper<T> GetDataMapper<T>() where T : class
        {
            var type = typeof(T);

            if (this.mappers == null)
            {
                this.mappers = new Dictionary<Type, object>();
            }
            if (!this.mappers.ContainsKey(typeof(T)))
            {
                this.mappers[type] = new DataMapper<T>(this);
            }
            return (DataMapper<T>)this.mappers[type];
        }

        private DataColumn[] GetColumnsFromTable(SqlConnection connection, string tableName)
        {
            List<DataColumn> columns = new List<DataColumn>();
            var columnsTable = new DataTable();
            var sqlQueryColumns = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'";
            adapter.SelectCommand = new SqlCommand(sqlQueryColumns, connection);
            adapter.Fill(columnsTable);

            var sqlQueryMaxId = $"SELECT MAX(Id) AS Id FROM {tableName}";
            SqlCommand cmd = new SqlCommand(sqlQueryMaxId, connection);
            var maxId = (int)cmd.ExecuteScalar() + 1;

            for (int j = 0; j < columnsTable.Rows.Count; j++)
            {
                var column = new DataColumn(columnsTable.Rows[j].ItemArray[0].ToString());
                if (column.ColumnName == "Id")
                {
                    column.AutoIncrement = true;
                    column.AutoIncrementSeed = maxId + 1;
                    column.AutoIncrementStep = 1;
                    column.Unique = true;
                }
                columns.Add(column);
            }
            return columns.ToArray();
        }

        public DataTable GetTable(string tableName)
        {
            return TablesWithData.Tables[tableName];
        }

        public DataTable GetTableWithData(string tableName/*, string sqlQuery*/)
        {
            var sqlQuery = $"SELECT * FROM {tableName}";
            //TODO: переделать запрос
            sqlQuery = $@"SELECT
                            BattleFields.Id,
                            BattleFields.SideLength,
                            Cells.Id,
                            Cells.ShipId,
                            Cells.BattleFieldID,
                            Cells.PointID,
                            Points.X,
                            Points.Y,
                            Ships.Id,
                            Ships.TypeId,
                            Ships.Velocity,
                            Ships.[Range],
                            Ships.Size
                        FROM BattleFields
                            LEFT JOIN Cells ON Cells.BattleFieldID = BattleFields.Id
                            LEFT JOIN Ships on Ships.id = cells.shipid
                            LEFT JOIN points on Points.Id = Cells.PointID";
            return GetDataFromDbTable(tableName, sqlQuery);
        }

        //public DataTable GetTableWithData(string tableName, int? id)
        //{
        //    var sqlQuery = $"SELECT * FROM {tableName} WHERE {tableName}.Id = {id}";
        //    return GetDataFromDbTable(tableName, sqlQuery);
        //}

        private DataTable GetDataFromDbTable(string tableName, string sqlQuery)
        {
            DataTable dataTable = new DataTable(tableName);
            SqlConnection connection = new SqlConnection(connectionString);
            using (connection)
            {
                try
                {
                    connection.Open();
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
            try
            {
                using (sqlConnection = new SqlConnection(connectionString))
                {
                    #region old DataAdapter.Update()

                    //sqlConnection.Open();
                    //for (int i = 0; i < TablesWithData.Tables.Count; i++)
                    //{
                    //    adapter.SelectCommand = new SqlCommand($"SELECT * FROM {TablesWithData.Tables[i].TableName}", sqlConnection);
                    //    SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
                    //    adapter.Update(TablesWithData, TablesWithData.Tables[i].TableName);
                    //}

                    #endregion old DataAdapter.Update()

                    Add(sqlConnection);
                    Update(sqlConnection);
                    Delete(sqlConnection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Add(SqlConnection sqlConnection)
        {
            var test = mappers.Values.First();

            var res = ((IDataMapper<>)test).mappedItems.First();
            var bla = res

            //for (int i = 0; i < mappers.Count; i++)
            //{
            //    IDataMapper mapper = mappers[i];
            //}
            //SqlGenerator.GetInsertIntoString();
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