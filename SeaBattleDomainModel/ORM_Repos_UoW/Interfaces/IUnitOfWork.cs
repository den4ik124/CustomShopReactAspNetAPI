using System;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /*int*/

        void Register(IBaseRepository repository);

        void Commit();
    }
}