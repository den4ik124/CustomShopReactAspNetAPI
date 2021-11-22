using ORM_Repos_UoW.Interfaces;
using System;
using System.Collections.Generic;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T> where T : class
    {
        private IDataMapper<T> dataMapper;

        public GenericRepos(DbContext context)
        {
            this.dataMapper = context.GetDataMapper<T>();
        }

        public void Create(T item)
        {
            dataMapper.Add(item);
        }

        public void Create(IEnumerable<T> items)
        {
            dataMapper.Add(items);
        }

        public T ReadItemById(int id)
        {
            CheckNumberOfItems();
            return dataMapper.ReadItemById(id);
        }

        public IEnumerable<T> ReadItems()
        {
            CheckNumberOfItems();
            return dataMapper.ReadAllItems();
        }

        private void CheckNumberOfItems()
        {
            if (dataMapper.ItemsCount == 0)
            {
                dataMapper.FillItems();
            }
        }

        public void Update(T item)
        {
            dataMapper.Update(item);
        }

        public void Delete(int id)
        {
            dataMapper.Delete(id);
        }

        public void Delete(T item)
        {
            dataMapper.Delete(item);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}