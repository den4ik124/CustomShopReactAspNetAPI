using System;
using System.Collections.Generic;
using System.Data;
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
        }
        public void Create(List<T> items)
        {
            var dataMapper = new DataMapper<T>(dbContext,items);
        }


        //public DataTable GetTable(System.Reflection.CustomAttributeData attributes)
        //{
        //    //var test = dbContext.GetTable(GetTableName(attributes));
        //    //return dbContext.GetTable(GetTableName(attributes));
        //}

        //private string? GetTableName(System.Reflection.CustomAttributeData attributes) //TODO: исправить на private вне тестов
        //{
        //    List<string> tablesNames = new List<string>();
        //    for (int i = 0; i < dbContext.tablesWithData.Tables.Count; i++)
        //    {
        //        tablesNames.Add(dbContext.tablesWithData.Tables[i].TableName);
        //    }
        //    var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value.ToString() == arg);
        //    return tableName;
        //}

        public T ReadItem(int id)
        {
            T item;
            using (var dataMapper = new DataMapper<T>(dbContext))
            {
                item = dataMapper.Items[0];
            }
            return item;
        }

        public IEnumerable<T> ReadItems()
        {
            IEnumerable<T> items;
            using (var dataMapper = new DataMapper<T>(dbContext))
            {
                items = dataMapper.Items;
            }
            return items;
            //var dataMapper = new DataMapper<T>(dbContext).Items;
            //return dataMapper.Items;
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