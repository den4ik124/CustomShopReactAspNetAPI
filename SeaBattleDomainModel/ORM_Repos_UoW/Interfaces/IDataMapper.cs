using System.Collections.Generic;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IDataMapper<T> where T : class
    {
        void Add(T item);

        void Add(IEnumerable<T> items);

        T ReadItemById(int id);

        IEnumerable<T> ReadAllItems();

        void Delete(int id);

        void FillItems();

        int ItemsCount { get; }
    }
}