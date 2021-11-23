﻿using ORM_Repos_UoW.Attributes;
using ORM_Repos_UoW.Enums;
using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T>// where T : class
    {
        private List<T> addedItems = new List<T>();
        private List<T> dirtyItems = new List<T>();
        private List<T> deletedItems = new List<T>();

        private IUnitOfWork unitOfWork;

        public GenericRepos(IUnitOfWork uow)
        {
            unitOfWork = uow;
            uow.Register(this);
        }

        public void Create(T item)
        {
            addedItems.Add(item);
        }

        public void Create(IEnumerable<T> items)
        {
            addedItems.AddRange(items);
        }

        public T ReadItemById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T?> ReadItems<T>()
        {
            var result = new List<T?>();
            var type = typeof(T);
            var tableName = type.GetCustomAttribute<TableAttribute>().TableName;

            //string sqlQuery = GetSqlQuery(type);

            var sqlQuery = $"SELECT * FROM {tableName}"; //TODO: генерировать SQL запрос здесь
            using (SqlConnection connection = new SqlConnection(unitOfWork.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add((T)MatchDataItem(type, reader));
                    }
                }
            }
            return result;
        }

        private string GetSqlQuery(Type type)
        {
            //List<string> tables = new List<string>();
            //tables.Add(type.GetCustomAttribute<TableAttribute>().TableName);
            //tables.AddRange(type.GetProperties().Select(prop => prop.GetCustomAttribute<ChildAttribute>().Table).ToList());

            //var columns =
            throw new NotImplementedException();
        }

        private object MatchDataItem(Type type, SqlDataReader reader)
        {
            var item = Activator.CreateInstance(type);
            var columns = type.GetProperties()
                                .Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0)
                                .Select(atr => atr.GetCustomAttribute<ColumnAttribute>().ColumnName);
            var properties = type.GetProperties().Where(prop => prop.GetCustomAttributes<ColumnAttribute>().Count() > 0);

            foreach (var prop in properties)
            {
                string columnName = prop.GetCustomAttribute<ColumnAttribute>().ColumnName;
                prop.SetValue(item, reader[columnName]);
            }
            return item;
        }

        public void Update(T item)
        {
            dirtyItems.Add(item);
        }

        public void Delete(int id)
        {
            T item = default(T); //продумать алгоритм поиска сущности по Id
            deletedItems.Add(item);
        }

        public void Delete(T item)
        {
            deletedItems.Add(item);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Submit(SqlConnection connection)
        {
            SubmitAddedItems(connection);
            SubmitDirtyItems(connection);
            SubmitDeletedItems(connection);
        }

        private static void SubmitAddedItems(SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private static void SubmitDirtyItems(SqlConnection connection)
        {
            throw new NotImplementedException();
        }

        private void SubmitDeletedItems(SqlConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}