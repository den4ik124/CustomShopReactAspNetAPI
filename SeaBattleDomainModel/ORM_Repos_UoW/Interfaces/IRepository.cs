using ORM_Repos_UoW.UoW_POC;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IRepository<T> : IBaseRepository, IDisposable// where T : class
    {
        void Create(T item);

        void Create(IEnumerable<T> items);

        void Update(T item);

        T ReadItemById(int id);

        IEnumerable<T> ReadItems<T>();

        void Delete(int id);

        void Delete(T item);
    }
}