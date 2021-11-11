using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW
{
    public interface IRepository<T> : IDisposable where T : class
    {
        IEnumerable<T> GetItems();

        T GetItem(int id);

        void Create(T item);

        void Delete(int id);
    }
}