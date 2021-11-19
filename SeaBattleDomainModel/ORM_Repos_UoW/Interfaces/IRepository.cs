using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    public interface IRepository<T> : IDisposable where T : class
    {
        void Create(T item);

        T ReadItem(int id);

        IEnumerable<T> ReadItems();

        void Delete(int id);
    }
}