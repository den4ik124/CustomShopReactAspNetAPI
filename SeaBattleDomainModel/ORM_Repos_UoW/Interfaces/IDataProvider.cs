using ORM_Repos_UoW.DataMapper;
using System.Collections.Generic;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IDataProvider<T> where T : class
    {
        public List<MappedItem<T>> MappedItems { get; set; }

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