﻿using OrmRepositoryUnitOfWork.Interfaces;
using OrmRepositoryUnitOfWork.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace OrmRepositoryUnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private Dictionary<string, IBaseRepository> repositories;
        private readonly ILogger logger;
        private bool disposed = false;
        private readonly SqlConnection sqlConnection;

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
                GetRepository<TInsert>().Create(ref item);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void CreateItems<TInsert>(IEnumerable<TInsert> items)
        {
            try
            {
                GetRepository<TInsert>().CreateItems(items);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public TRead ReadItem<TRead>(int id)
        {
            try
            {
                var item = GetRepository<TRead>().ReadItemById(id);
                return item;
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
            return default(TRead);
        }

        public IEnumerable<TRead> ReadItems<TRead>()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                return GetRepository<TRead>().ReadItems();
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
            sw.Stop();
            Console.WriteLine("ReadItems :" + sw.ElapsedMilliseconds + " ms.");
            return default(IEnumerable<TRead>);
        }

        public void Update<TUpdate>(TUpdate item)
        {
            try
            {
                GetRepository<TUpdate>().Update(item);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void Delete<TDelete>(TDelete item)
        {
            try
            {
                GetRepository<TDelete>().Delete(item);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void Delete<TDelete>(string columnName, dynamic value)
        {
            try
            {
                GetRepository<TDelete>().Delete(columnName, value);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void DeleteById<TDelete>(int id)
        {
            try
            {
                GetRepository<TDelete>().DeleteById(id);
            }
            catch (Exception ex)
            {
                this.logger.Log(ex.Message);
            }
        }

        public void Commit()
        {
            try
            {
                var transaction = this.sqlConnection.BeginTransaction();

                try
                {
                    this.repositories.ToList().ForEach(x => x.Value.Submit(transaction));

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
                this.repositories[type.Name] = new GenericRepos<T>(this.sqlConnection);
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
                    this.sqlConnection.Dispose();
                    foreach (var repos in this.repositories.Values)
                    {
                        repos.Dispose();
                    }
                }
                disposed = true;
            }
        }
    }
}