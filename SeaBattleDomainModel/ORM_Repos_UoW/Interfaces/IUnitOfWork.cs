using System;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
    }
}