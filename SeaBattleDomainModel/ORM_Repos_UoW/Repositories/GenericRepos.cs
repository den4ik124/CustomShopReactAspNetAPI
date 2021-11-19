using System;
using System.Collections.Generic;
using System.Linq;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T> where T : class
    {
        private DbContext dbContext;
        private DataMapper<T> dataMapper;

        public GenericRepos(DbContext context)
        {
            this.dbContext = context;
            this.dataMapper = new DataMapper<T>(context);
        }

        public void Create(T item)
        {
            dataMapper.MappedItems.Add(new MappedItem<T>(item, State.Added));
            //dataMapper.TransferItemsIntoDbTable();
        }

        public void Create(List<T> items)
        {
            dataMapper.MappedItems.AddRange(items.Select(item => new MappedItem<T>(item, State.Added)));

            //dataMapper.TransferItemsIntoDbTable();
        }

        public T ReadItem(int id)
        {
            CheckNumberOfItems();

            //var item = dataMapper.MappedItems.Select(i => i.Item).First();
            //var type = item.GetType();
            //var property = type.GetProperty("Id");
            //var value = property.GetValue(item);
            return dataMapper.MappedItems.Select(i => i.Item)
                                         .FirstOrDefault(item => (int)item.GetType()
                                         .GetProperty("Id")
                                         .GetValue(item) == id);
        }

        public IEnumerable<T> ReadItems()
        {
            CheckNumberOfItems();
            return dataMapper.MappedItems.Select(e => e.Item);
        }

        private void CheckNumberOfItems()
        {
            if (dataMapper.MappedItems.Count == 0)
            {
                dataMapper.FillItems();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}