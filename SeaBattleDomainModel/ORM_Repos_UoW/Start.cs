using Microsoft.EntityFrameworkCore;
using SeaBattleDomainModel.Entities;
using System;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext _dbContext;
        public IRepository<Ship> ShipRepository { get; set; }

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            ShipRepository = new ShipRepository(dbContext);
        }

        public int Commit()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    internal class Start
    {
        private UnitOfWork UnitOfWork = new UnitOfWork(/*TODO: здесь должна быть ORM ?*/);
    }
}