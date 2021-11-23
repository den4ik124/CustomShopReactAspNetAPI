using System.Data.SqlClient;

namespace ORM_Repos_UoW.Interfaces
{
    public interface IBaseRepository
    {
        void Submit(SqlConnection connection);
    }
}