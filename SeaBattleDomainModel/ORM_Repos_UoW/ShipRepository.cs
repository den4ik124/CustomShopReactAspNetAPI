using Microsoft.EntityFrameworkCore;
using SeaBattleDomainModel.Entities;
using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    public class ShipRepository : IRepository<Ship>
    {
        private DbContext dbContext;
        public ShipRepository(DbContext context) //сюда должен передаваться context из ORM
        {
            this.dbContext = context;
        }

        public void Create(Ship item)
        {
            this.dbContext.Add(item);
        }
        public Ship GetItem(int id)
        {
            return (Ship)this.dbContext.Find(typeof(Ship), id);
        }

        public void Delete(int id)
        {
            this.dbContext.Remove(id);
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }


        public IEnumerable<Ship> GetItems()
        {
            return this.dbContext;
        }
    }
}