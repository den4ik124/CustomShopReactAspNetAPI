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

        public void Create(List<T> items)
        {
            dataMapper.Add(items);
        }

        public T ReadItemById(int id)
        {
            CheckNumberOfItems();

            //var item = dataMapper.MappedItems.Select(i => i.Item).First();
            //var type = item.GetType();
            //var property = type.GetProperty("Id");
            //var value = property.GetValue(item);
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

        public void Delete(int id)
        {
            dataMapper.Delete(id);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}