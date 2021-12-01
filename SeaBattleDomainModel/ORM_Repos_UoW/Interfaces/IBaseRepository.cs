using System;
using System.Data.SqlClient;

namespace OrmRepositoryUnitOfWork.Interfaces
{
    public interface IBaseRepository
    {
        void Submit(SqlConnection connection, SqlTransaction transaction);
    }
}