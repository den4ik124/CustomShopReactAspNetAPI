using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Repos_UoW
{
    public class MappedItem<T> where T : class
    {
        public MappedItem(T item, State state)
        {
            Item = item;
            State = state;
        }

        public State State { get; set; }

        public T Item { get; set; }
    }
}