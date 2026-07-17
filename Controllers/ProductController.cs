using ASP_FinancialProductWishList.Services.Interfaces;
using ASP_FinancialProductWishList.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASP_FinancialProductWishList.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var products = await _productService.GetListAsync(cancellationToken);

            var viewModels = products
                .Select(product => new ProductListItemViewModel
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    FeeRate = product.FeeRate,
                })
                .ToList();

            return View(viewModels);
        }
    }
}
