using System.Collections.Generic;
using System.Data.SqlClient;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IRepository<T> : IBaseRepository
    {
        void Create<TItem>(ref TItem item, SqlConnection connection);

        void Create(IEnumerable<T> items, SqlConnection connection);

        void Update(T item);

        void UpdateBy(T item, string columnName, object value);

        T ReadItemById(int id);

        IEnumerable<T> ReadItems();

        void DeleteById(int id);

        void Delete(T item);

        void Delete(string columnName, object value);
    }
}