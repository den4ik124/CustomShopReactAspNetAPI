using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IRepository<T> : IBaseRepository
    {
        //void Create(T item); //TODO: вернуть этот метод, перед слиянием
        void Create<TItem>(TItem item, SqlConnection connection);

        void Create(IEnumerable<T> items);

        void Update(T item);

        void UpdateBy(T item, string columnName, object value);

        T ReadItemById(int id);

        IEnumerable<T> ReadItems();

        void Delete(int id);

        void Delete(T item);

        void Delete(string columnName, object value);
    }
}