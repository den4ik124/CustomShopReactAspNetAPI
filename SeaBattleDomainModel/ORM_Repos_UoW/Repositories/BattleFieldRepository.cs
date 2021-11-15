using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    internal class BattleFieldRepository : IRepository<BattleField>
    {
        private DbContext dbContext;

        //private List<Ship> addedShips = new List<Ship>();
        //private List<Ship> dirtyShips = new List<Ship>();
        //private List<Ship> removedShips = new List<Ship>();

        public BattleFieldRepository(DbContext context) //сюда должен передаваться context из ORM
        {
            this.dbContext = context;
        }

        public void Create(BattleField item)
        {
            throw new NotImplementedException();
        }

        public BattleField GetItem(int id)
        {
            throw new NotImplementedException();

        }
        public IEnumerable<BattleField> GetItems()
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}