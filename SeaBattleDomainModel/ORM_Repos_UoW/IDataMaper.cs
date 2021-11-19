using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    public interface IDataMaper<T> where T : class
    {
        void Add(T item);

        void Add(IEnumerable<T> items);

        T ReadItem(int id);

        IEnumerable<T> ReadAllItems();

        void Delete(int id);
    }
}