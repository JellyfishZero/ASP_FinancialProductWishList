using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace ASP_FinancialProductWishList.Repositories
{
    /// <summary>
    /// Sql資料庫連接工廠
    /// </summary>
    public sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
