using System.Collections.Generic;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IDataMapper<T> where T : class
    {
        void Add(T item);

        void Add(IEnumerable<T> items);

        T ReadItemById(int id);

        IEnumerable<T> ReadAllItems();

        void Update(T item);

        void Delete(int id);

        void Delete(T item);

        void FillItems();

        int ItemsCount { get; }
    }
}