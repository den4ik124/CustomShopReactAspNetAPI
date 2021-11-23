using ORM_Repos_UoW.Interfaces;
using ORM_Repos_UoW.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext dbContext;
        private Dictionary<string, IBaseRepository> _repositories;

        //public IBaseRepository Ships { get; set; }

        //public Dictionary<Type, object> Repositories { get; set; }

        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
            _repositories = new Dictionary<string, IBaseRepository>();
        }

        public void Register(IBaseRepository repository)
        {
            _repositories.Add(repository.GetType().Name, repository);
        }

        public void Commit()
        {
            using (SqlConnection connection = new SqlConnection("connection string"))
            {
                _repositories.ToList().ForEach(x => x.Value.Submit(connection));
            }
        }

        public IRepository<T> GetRepository<T>() where T : class //TODO: поменять GenericRepos на IRepository
        {
            return new GenericRepos<T>(this);
        }

        //public GenericRepos<T> GetRepository<T>() where T : class //TODO: поменять GenericRepos на IRepository
        //{
        //    var type = typeof(T);
        //    if (Repositories == null)
        //    {
        //        Repositories = new Dictionary<Type, object>();
        //    }
        //    if (!Repositories.ContainsKey(typeof(T)))
        //    {
        //        Repositories[type] = new Repositories.GenericRepos<T>(dbContext);
        //    }
        //    return (GenericRepos<T>)Repositories[type];
        //}

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}