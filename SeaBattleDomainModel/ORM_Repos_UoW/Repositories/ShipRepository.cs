using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class ShipRepository : IRepository<Ship>
    {
        private DbContext dbContext;

        //private List<Ship> addedShips = new List<Ship>();
        //private List<Ship> dirtyShips = new List<Ship>();
        //private List<Ship> removedShips = new List<Ship>();

        public ShipRepository(DbContext context) //сюда должен передаваться context из ORM
        {
            this.dbContext = context;
        }

        public void Create(Ship item)
        {
            this.dbContext.ShipsList.Items.Add(item, State.Added);
        }

        public Ship ReadItem(int id)
        {
            return this.dbContext.ShipsList.Items.First(item => item.Key.Id == id).Key;
        }

        public void Delete(int id)
        {
            var ship = this.dbContext.ShipsList.Items.FirstOrDefault(item => item.Key.Id == id).Key;
            if (ship != null)
                this.dbContext.ShipsList.Items.Remove(ship);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Ship> ReadItems()
        {
            return dbContext.ShipsList.Items.Select(item => item.Key);
        }
    }
}