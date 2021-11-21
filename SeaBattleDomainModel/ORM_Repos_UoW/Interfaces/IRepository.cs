using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class
    {
        void Create(T item);

        T ReadItemById(int id);

        IEnumerable<T> ReadItems();

        void Delete(int id);
    }
}