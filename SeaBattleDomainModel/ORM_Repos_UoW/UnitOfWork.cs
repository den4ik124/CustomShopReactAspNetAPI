using ORM_Repos_UoW.Interfaces;
using ORM_Repos_UoW.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private Dictionary<string, IBaseRepository> _repositories;

        public string ConnectionString { get; set; }

        public UnitOfWork()
        {
            _repositories = new Dictionary<string, IBaseRepository>();
        }

        public void Create<T>(T item)
        {
            GetRepository<T>().Create(item);
        }

        public void Create<T>(IEnumerable<T> items)
        {
            GetRepository<T>().Create(items);
        }

        public T ReadItem<T>(int id)
        {
            return GetRepository<T>().ReadItemById(id);
        }

        public IEnumerable<T> ReadItems<T>()
        {
            return GetRepository<T>().ReadItems();
        }

        public void Update<T>(T item)
        {
            GetRepository<T>().Update(item);
        }

        public void Delete<T>(T item)
        {
            GetRepository<T>().Delete(item);
        }

        public void Delete<T>(int id)
        {
            GetRepository<T>().Delete(id);
        }

        //public void Register(IBaseRepository repository)
        //{
        //    _repositories.Add(repository.GetType().Name, repository);
        //}

        public void Commit()
        {
            SqlTransaction transaction;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    try
                    {
                        _repositories.ToList().ForEach(x => x.Value.Submit(connection, transaction));

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        transaction.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //public IRepository<T> GetRepository<T>()// where T : class, struct //TODO: поменять GenericRepos на IRepository
        //{
        //    return new GenericRepos<T>(this);
        //}

        private GenericRepos<T> GetRepository<T>()
        {
            var type = typeof(T);
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, IBaseRepository>();
            }
            if (!_repositories.ContainsKey(typeof(T).Name))
            {
                _repositories[type.Name] = new GenericRepos<T>(this);
            }
            return (GenericRepos<T>)_repositories[type.Name];
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}