using System.Collections.Generic;
using System.Linq;

namespace ORM_Repos_UoW
{
    public class DbSet<T> where T : class
    {
        public Dictionary<T, State> Items { get; }

        public DbSet()
        {
            Items = new Dictionary<T, State>();
        }

        public void Add(T item)
        {
            Items.Add(item, State.Added);
        }

        public void Update(T item)
        {
            if (Items.Keys.Contains(item))
            {
                Items[item] = State.Modified;
            }
        }
        public void Delete(T item)
        {
            if (Items.Keys.Contains(item))
            {
                Items[item] = State.Deleted;
            }
        }
    }
}