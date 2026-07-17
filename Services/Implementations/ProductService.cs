using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;
using ASP_FinancialProductWishList.Services.Interfaces;

namespace ASP_FinancialProductWishList.Services.Implementations
{
    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Task<IReadOnlyList<Product>> GetListAsync(
            CancellationToken cancellationToken = default
        )
        {
            return _productRepository.GetListAsync(cancellationToken);
        }

        public Task<Product?> GetByIdAsync(
            int productID,
            CancellationToken cancellationToken = default
        )
        {
            if (productID <= 0)
            {
                return Task.FromResult<Product?>(null);
            }

            return _productRepository.GetByIdAsync(productID, cancellationToken);
        }
    }
}
