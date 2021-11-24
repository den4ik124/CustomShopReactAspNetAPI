using System;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /*int*/
        string ConnectionString { get; set; }
        //void Register(IBaseRepository repository);

        void Commit();
    }
}