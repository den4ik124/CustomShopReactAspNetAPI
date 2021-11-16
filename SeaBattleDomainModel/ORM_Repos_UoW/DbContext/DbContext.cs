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
        public DbSet<Ship> ShipsList { get; set; }

        public SqlDataAdapter Adapter { get; set; }
        public DataSet TablesWithData { get; set; }
        //private DataRow dr;

        public DbContext(string connection)
        {
            sqlConnection = new SqlConnection(connection);
            ShipsList = new DbSet<Ship>();
            Adapter = new SqlDataAdapter();
            TablesWithData = new DataSet();

            Preparing(sqlConnection);

            //CellsList = new List<Cell>();
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
                        Adapter.SelectCommand = new SqlCommand(sql, connection);
                        Adapter.Fill(table);
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
                Adapter.Update(TablesWithData);

                //Add(sqlConnection);
                //Update(sqlConnection);
                //Remove(sqlConnection);
            }
            throw new NotImplementedException();
        }

        private void Add(SqlConnection sqlConnection)
        {
            var added = ShipsList.Items.Where(item => item.Value == State.Added).Select(item => item.Key).ToList();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            var propsAndValues = new Dictionary<string, object>(); //TODO: реализовать заполнение словаря
            //sqlCommand.CommandText = SqlGenerator.GetInsertIntoString(test, "table");
            throw new NotImplementedException();
        }

        private void Update(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }

        private void Remove(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }
    }
}