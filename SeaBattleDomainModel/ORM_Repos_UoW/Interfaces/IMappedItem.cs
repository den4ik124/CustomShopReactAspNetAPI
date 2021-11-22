using System.Data;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IMappedItem<T> where T : class
    {
        public State State { get; set; }

        public DataRow Row { get; set; }

        public T Item { get; set; }
    }
}