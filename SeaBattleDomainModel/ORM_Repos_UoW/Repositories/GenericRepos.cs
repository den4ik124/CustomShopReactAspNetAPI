using System;
using System.Collections.Generic;
using System.Linq;

namespace ORM_Repos_UoW.Repositories
{
    public class GenericRepos<T> : IRepository<T> where T : class
    {
        private DbContext dbContext;

        public GenericRepos(DbContext context)
        {
            this.dbContext = context;
        }

        public void Create(T item)
        {
            var dataMapper = new DataMapper<T>(dbContext, item);
            dataMapper.TransferItemsIntoDbTable();
        }

        public void Create(List<T> items)
        {
            var dataMapper = new DataMapper<T>(dbContext, items);
            dataMapper.TransferItemsIntoDbTable();
        }

        public T ReadItem(int id)
        {
            var dataMapper = new DataMapper<T>(dbContext);
            dataMapper.FillItems(id);

            return dataMapper.Items[0];

            //foreach (var elem in dataMapper.Items)
            //{
            //    if ((int)typeof(T).GetProperties()
            //                        .FirstOrDefault(prop => prop.Name == "Id")
            //                        .GetValue(elem) == id)
            //    {
            //        return elem;
            //    }
            //}
            //return null; //TODO: избавиться от return null;
        }

        public IEnumerable<T> ReadItems()
        {
            var dataMapper = new DataMapper<T>(dbContext);
            dataMapper.FillItems();
            return dataMapper.Items;
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