using System;

namespace ORM_Repos_UoW.Interfaces
{
    internal interface IUnitOfWork : IDisposable
    {
        /*int*/

        void Commit();
    }
}