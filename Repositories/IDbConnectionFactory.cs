using System.Data.Common;

namespace ASP_FinancialProductWishList.Repositories
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
