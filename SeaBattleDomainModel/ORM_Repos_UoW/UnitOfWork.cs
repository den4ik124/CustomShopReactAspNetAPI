using SeaBattleDomainModel.Entities;
using System;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private Context dbContext;
        public IRepository<Ship> ShipRepository { get; set; }

        public UnitOfWork(Context dbContext)
        {
            this.dbContext = dbContext;
            ShipRepository = new ShipRepository(dbContext);
        }

        public int Commit()
        {
            throw new NotImplementedException();
            //return dbContext.SaveChanges();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}