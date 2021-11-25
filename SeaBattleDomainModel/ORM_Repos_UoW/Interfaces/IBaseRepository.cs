using System;
using System.Data.SqlClient;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IBaseRepository : IDisposable
    {
        void Submit(SqlConnection connection, SqlTransaction transaction);
    }
}