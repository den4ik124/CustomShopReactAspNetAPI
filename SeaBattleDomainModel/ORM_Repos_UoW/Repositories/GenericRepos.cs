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
            dataMapper.TransferItemsIntoDB();
        }
        public void Create(List<T> items)
        {
            var dataMapper = new DataMapper<T>(dbContext, items);
            dataMapper.TransferItemsIntoDB();
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
            var dataMapper = new DataMapper<T>(dbContext);
            dataMapper.FillItems();

            foreach (var elem in dataMapper.Items)
            {
                if ((int)typeof(T).GetProperties()
                    .FirstOrDefault(prop => prop.Name == "Id")
                    .GetValue(elem) == id)
                {
                    return elem; 
                }
            }
            return null; //TODO: избавиться от return null;
        }

        public IEnumerable<T> ReadItems()
        {
            var dataMapper = new DataMapper<T>(dbContext);
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