using System;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        string ConnectionString { get; }

        void Commit();
    }
}