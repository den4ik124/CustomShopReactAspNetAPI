using System;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        string ConnectionString { get; }

        void Commit();
    }
}