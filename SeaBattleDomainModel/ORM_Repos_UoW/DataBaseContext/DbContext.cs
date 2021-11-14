using SeaBattleDomainModel.Entities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class DbContext
    {
        private SqlConnection sqlConnection;
        public DbSet<Ship> ShipsList { get; }

        private SqlDataAdapter da;
        private DataTable dt;
        private DataRow dr;
        //public List<Cell> CellsList { get; set; }
        //SqlDataAdapter SqlDataAdapter = new SqlDataAdapter();
        //DataTable dt = new DataTable();

        public DbContext(string connection)
        {
            sqlConnection = new SqlConnection(connection);
            ShipsList = new DbSet<Ship>();
            da = new SqlDataAdapter();
            dt = new DataTable();
            dr = dt.NewRow();
            //CellsList = new List<Cell>();
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
            var test = ShipsList.Items;
            //var testSelect = test.Select(item => item.Key);
            //var testWhere = test.Where(item => item.Value == State.Added).ToList();
            //var testWhereSelect = testWhere.Select(item => item.Key).ToList();
            var added = ShipsList.Items.Where(item => item.Value == State.Added).Select(item => item.Key).ToList();

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