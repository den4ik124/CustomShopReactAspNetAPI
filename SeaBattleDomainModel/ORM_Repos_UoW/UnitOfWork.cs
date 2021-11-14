using SeaBattleDomainModel.Entities;
using System;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext dbContext;
        public IRepository<Ship> ShipRepository { get; }
        public IRepository<Cell> CellRepository { get; }

        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
            ShipRepository = new ShipRepository(dbContext);
            //CellRepository = new CellRepository(dbContext);
        }

        public int Commit()
        {
            return dbContext.SaveChanges();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}