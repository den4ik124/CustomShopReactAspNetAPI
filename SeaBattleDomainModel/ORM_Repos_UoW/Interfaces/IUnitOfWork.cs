using System;
using System.Collections.Generic;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Create<TInsert>(TInsert item);

        void CreateItems<TInsert>(IEnumerable<TInsert> items);

        TRead ReadItem<TRead>(int id);

        IEnumerable<TRead> ReadItems<TRead>(string columnName = "", object? value = null);

        void Update<TUpdate>(TUpdate item);

        void Delete<TDelete>(TDelete item);

        void Delete<TDelete>(string columnName, dynamic value);

        void DeleteById<TDelete>(int id);

        void Commit();
    }
}