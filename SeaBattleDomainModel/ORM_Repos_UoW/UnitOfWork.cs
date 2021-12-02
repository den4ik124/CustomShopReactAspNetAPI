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
        private bool disposed = false;
        private SqlConnection sqlConnection;

        public UnitOfWork(string connectionString, ILogger logger)
        {
            this.repositories = new Dictionary<string, IBaseRepository>();
            this.logger = logger;

            this.sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }

        public void Create<TInsert>(TInsert item)
        {
            try
            {
                GetRepository<TInsert>().Create(ref item, this.sqlConnection);
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
                GetRepository<TInsert>().Create(items, this.sqlConnection);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public TRead ReadItem<TRead>(int id)
        {
            return GetRepository<TRead>().ReadItemById(id, this.sqlConnection);
        }

        public IEnumerable<TRead> ReadItems<TRead>()
        {
            return GetRepository<TRead>().ReadItems(this.sqlConnection);
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
            try
            {
                var transaction = this.sqlConnection.BeginTransaction();

                try
                {
                    this.repositories.ToList().ForEach(x => x.Value.Submit(this.sqlConnection, transaction));

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    this.logger.Log(ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception innerEx)
                    {
                        this.logger.Log(innerEx.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        private IRepository<T> GetRepository<T>()
        {
            var type = typeof(T);

            if (!this.repositories.ContainsKey(typeof(T).Name))
            {
                this.repositories[type.Name] = new GenericRepos<T>(this.logger);
            }
            return (IRepository<T>)this.repositories[type.Name];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    sqlConnection.Dispose();
                }
                disposed = true;
            }
        }
    }
}