using System;
using System.Data.SqlClient;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IBaseRepository : IDisposable
    {
        void Submit(/*SqlConnection connection, */SqlTransaction transaction);
    }
}