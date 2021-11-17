using System;

namespace ORM_Repos_UoW
{
    internal interface IUnitOfWork : IDisposable
    {
        /*int*/

        void Commit();
    }
}