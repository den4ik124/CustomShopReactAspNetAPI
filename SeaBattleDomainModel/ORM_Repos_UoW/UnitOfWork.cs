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

        public string ConnectionString { get; }

        public UnitOfWork(string connectionString)
        {
            ConnectionString = connectionString;
            _repositories = new Dictionary<string, IBaseRepository>();
        }

        public void Create<TInsert>(TInsert item)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                GetRepository<TInsert>().Create(ref item, connection);
            }
        }

        public void Create<TInsert>(IEnumerable<TInsert> items)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                GetRepository<TInsert>().Create(items, connection);
            }
        }

        public TRead ReadItem<TRead>(int id)
        {
            return GetRepository<TRead>().ReadItemById(id);
        }

        public IEnumerable<TRead> ReadItems<TRead>()
        {
            return GetRepository<TRead>().ReadItems();
        }

        public void Update<TUpdate>(TUpdate item)
        {
            GetRepository<TUpdate>().Update(item);
        }

        public void Delete<TDelete>(TDelete item)
        {
            GetRepository<TDelete>().Delete(item);
        }

        public void Delete<TDelete>(int id)
        {
            GetRepository<TDelete>().Delete(id);
        }

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
                //TODO: use some logger instead of Debug
                Debug.WriteLine(ex.Message);
            }
        }

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