using ORM_Repos_UoW.Repositories;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext dbContext;

        private Dictionary<Type, object> Repositories { get; set; }

        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public GenericRepos<T> GetRepository<T>() where T : class //TODO: поменять GenericRepos на IRepository
        {
            var type = typeof(T);
            if (Repositories == null)
            {
                Repositories = new Dictionary<Type, object>();
            }
            if (!Repositories.ContainsKey(typeof(T)))
            {
                Repositories[type] = new GenericRepos<T>(dbContext);
            }
            return (GenericRepos<T>)Repositories[type];
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