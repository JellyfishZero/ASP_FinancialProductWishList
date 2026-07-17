using ASP_FinancialProductWishList.Models.Entities;

namespace ASP_FinancialProductWishList.Services.Interfaces
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetListAsync(CancellationToken cancellationToken = default);

        Task<Product?> GetByIdAsync(int productID, CancellationToken cancellationToken = default);
    }
}
