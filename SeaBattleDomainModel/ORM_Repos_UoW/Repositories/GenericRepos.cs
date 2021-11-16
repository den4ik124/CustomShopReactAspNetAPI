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
            var type = typeof(T);
            var attributes = type.GetCustomAttributesData().First(a => a.AttributeType.Name == "TableAttribute");//.ConstructorArguments;

            string? tableName = GetTableName(attributes);

            var foundedTable = dbContext.GetTable(tableName);
            //TODO: Реализовать вызов DataMapper здесь
            //найти в dbContext-e таблицу с  таким же атрибутом, как у класса T.
            //Записать данные в таблицу согласно указанным колонкам.
            throw new NotImplementedException();
        }


        public DataTable GetTable(System.Reflection.CustomAttributeData attributes)
        {
            var test = dbContext.GetTable(GetTableName(attributes));
            return dbContext.GetTable(GetTableName(attributes));
        }

        private string? GetTableName(System.Reflection.CustomAttributeData attributes) //TODO: исправить на private вне тестов
        {
            List<string> tablesNames = new List<string>();
            for (int i = 0; i < dbContext.TablesWithData.Tables.Count; i++)
            {
                tablesNames.Add(dbContext.TablesWithData.Tables[i].TableName);
            }
            var tableName = tablesNames.FirstOrDefault(arg => attributes?.ConstructorArguments[0].Value.ToString() == arg);
            return tableName;
        }

        public T ReadItem(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadItems()
        {
            throw new NotImplementedException();
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