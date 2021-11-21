using ORM_Repos_UoW.Interfaces;
using ORM_Repos_UoW.Repositories;
using System;
using System.Collections.Generic;

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

            //TODO: вставить здесь проверку на вложенные классы и вызов метода GetRepository

            return (GenericRepos<T>)Repositories[type];
        }

        public void Commit()
        {
            /*return */
            dbContext.SaveChanges();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}