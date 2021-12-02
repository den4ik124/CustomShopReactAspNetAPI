using OrmRepositoryUnitOfWork.Interfaces;
using OrmRepositoryUnitOfWork.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace OrmRepositoryUnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private Dictionary<string, IBaseRepository> repositories;
        private readonly ILogger logger;
        public string ConnectionString { get; }

        public UnitOfWork(string connectionString, ILogger logger)
        {
            this.ConnectionString = connectionString;
            this.repositories = new Dictionary<string, IBaseRepository>();
            this.logger = logger;
        }

        public void Create<TInsert>(TInsert item)
        {
            try
            {
                using (var connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();
                    GetRepository<TInsert>().Create(ref item, connection);
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void Create<TInsert>(IEnumerable<TInsert> items)
        {
            try
            {
                using (var connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();
                    GetRepository<TInsert>().Create(items, connection);
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
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

        public void DeleteById<TDelete>(int id)
        {
            GetRepository<TDelete>().DeleteById(id);
        }

        public void Commit()
        {
            SqlTransaction transaction;
            try
            {
                using (SqlConnection connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    try
                    {
                        this.repositories.ToList().ForEach(x => x.Value.Submit(connection, transaction));

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(ex.Message);
                        transaction.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        private GenericRepos<T> GetRepository<T>()
        {
            var type = typeof(T);
            if (this.repositories == null)
            {
                this.repositories = new Dictionary<string, IBaseRepository>();
            }
            if (!this.repositories.ContainsKey(typeof(T).Name))
            {
                this.repositories[type.Name] = new GenericRepos<T>(this.ConnectionString);
            }
            return (GenericRepos<T>)this.repositories[type.Name];
        }
    }
}