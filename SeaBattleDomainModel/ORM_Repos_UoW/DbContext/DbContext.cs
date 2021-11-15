using SeaBattleDomainModel.DerivedShips;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        public DbSet<Ship> ShipsList { get; }

        private SqlDataAdapter da;
        public DataTable shipsTable;
        public DataTable battleFieldsTable;
        private DataRow dr;

        public List<Ship> ShipList
        {
            get
            {
                List<Ship> ships = new List<Ship>();
                var rows = this.shipsTable.Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    switch (rows[i]["TypeId"])
                    {
                        case 1:
                            ships.Add(new BattleShip((int)rows[i]["Id"], (int)rows[i]["Velocity"], (int)rows[i]["Range"], (int)rows[i]["Size"]));
                            break;

                        case 2:
                            ships.Add(new RepairShip((int)rows[i]["Id"], (int)rows[i]["Velocity"], (int)rows[i]["Range"], (int)rows[i]["Size"]));
                            break;

                        case 3:
                            ships.Add(new ComboShip((int)rows[i]["Id"], (int)rows[i]["Velocity"], (int)rows[i]["Range"], (int)rows[i]["Size"]));
                            break;

                        default:
                            break;
                    }
                }
                return ships;
            }
        }

        public DbContext(string connection)
        {
            sqlConnection = new SqlConnection(connection);
            ShipsList = new DbSet<Ship>();
            da = new SqlDataAdapter();
            shipsTable = new DataTable();
            battleFieldsTable = new DataTable();

            Preparing(sqlConnection);

            //CellsList = new List<Cell>();
        }

        private void Preparing(SqlConnection connection)
        {
            try
            {
                da.SelectCommand = new SqlCommand(SqlGenerator.GetSelectAllString("Ships"), connection);
                da.Fill(shipsTable);
                da.SelectCommand = new SqlCommand(SqlGenerator.GetSelectAllString("BattleFields"), connection);
                da.Fill(battleFieldsTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int SaveChanges()
        {
            using (sqlConnection)
            {
                Add(sqlConnection);
                Update(sqlConnection);
                Remove(sqlConnection);
            }
            throw new NotImplementedException();
        }

        private void Add(SqlConnection sqlConnection)
        {
            //var added = ShipsList.Items.Where(item => item.Value == State.Added).Select(item => item.Key).ToList();

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