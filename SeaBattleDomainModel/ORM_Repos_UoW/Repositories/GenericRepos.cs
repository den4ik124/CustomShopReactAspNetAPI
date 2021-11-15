using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
             throw new NotImplementedException();
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
