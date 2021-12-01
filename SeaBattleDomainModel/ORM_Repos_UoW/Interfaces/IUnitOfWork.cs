using System;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IUnitOfWork
    {
        string ConnectionString { get; }

        void Commit();
    }
}