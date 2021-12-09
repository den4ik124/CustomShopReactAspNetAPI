using System.Collections.Generic;
using System.Data.SqlClient;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IRepository<T> : IBaseRepository
    {
        void Create<TItem>(ref TItem item);

        void CreateItems(IEnumerable<T> items);

        void Update(T item);

        void UpdateBy(T item, string columnName, object value);

        T ReadItemById(int id);

        IEnumerable<T> ReadItems(string columnName = "", object? value = null);

        void DeleteById(int id);

        void Delete(T item);

        void Delete(string columnName, object value);
    }
}