using System.Collections.Generic;

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
    }
}