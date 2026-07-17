using ASP_FinancialProductWishList.Models.Entities;

namespace ASP_FinancialProductWishList.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> GetListAsync(CancellationToken cancellationToken = default);

        Task<Product?> GetByIdAsync(int productID, CancellationToken cancellationToken = default);
    }
}
