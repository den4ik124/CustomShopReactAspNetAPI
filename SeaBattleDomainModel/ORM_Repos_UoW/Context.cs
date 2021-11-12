using SeaBattleDomainModel.DerivedShips;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ORM_Repos_UoW
{
    public class Context
    {
        private SqlConnection sqlConnection;
        public Context(string connection)
        {
            sqlConnection = new SqlConnection(connection);
        }

        public void Add(int id)
        {
            throw new NotImplementedException();
        }

        public void Find<T>(int id)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<BattleShip> FindAll<T>()
        {
            List<BattleShip> ships = new List<BattleShip>(); 
            using (sqlConnection)
            {
                sqlConnection.Open();
                var sqlSelectQuery = "SELECT * FROM SHIPS WHERE TypeId = 1";
                SqlCommand command = new SqlCommand(sqlSelectQuery, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    //0 Id 1 TypeId  2 Velocity 3 Range   4 Size
                    BattleShip bs = new BattleShip(reader.GetInt32(0), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));
                    ships.Add(bs);
                }
            }
            return ships;
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}