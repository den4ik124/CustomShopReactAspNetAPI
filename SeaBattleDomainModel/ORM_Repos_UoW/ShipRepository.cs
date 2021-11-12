using Microsoft.EntityFrameworkCore;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    public class ShipRepository : IRepository<Ship>
    {
        private Context dbContext;
        public ShipRepository(Context context) //сюда должен передаваться context из ORM
        {
            this.dbContext = context;
        }

        public void Create(Ship item)
        {
            throw new NotImplementedException();
            //this.dbContext.Add(item);
        }
        public Ship GetItem(int id)
        {
            throw new NotImplementedException();

            //return (Ship)this.dbContext.Find<Ship>(id);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();

            //this.dbContext.Remove(id);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<Ship> GetItems()
        {
            //dbContext.
            throw new NotImplementedException();   

            //return this.dbContext;
        }
    }
}