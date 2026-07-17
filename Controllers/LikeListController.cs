using System.Globalization;
using ASP_FinancialProductWishList.Common.Exceptions;
using ASP_FinancialProductWishList.Common.Extensions;
using ASP_FinancialProductWishList.Services.DTOs;
using ASP_FinancialProductWishList.Services.Interfaces;
using ASP_FinancialProductWishList.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_FinancialProductWishList.Controllers
{
    [Authorize]
    public sealed class LikeListController : Controller
    {
        private readonly ILikeListService _likeListService;
        private readonly IProductService _productService;

        public LikeListController(ILikeListService likeListService, IProductService productService)
        {
            _likeListService = likeListService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userID = User.GetRequiredUserID();

            var items = await _likeListService.GetListAsync(userID, cancellationToken);

            var viewModels = items
                .Select(item => new LikeListItemViewModel
                {
                    LikeListID = item.LikeListID,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    FeeRate = item.FeeRate,
                    Quantity = item.Quantity,
                    MaskedDebitAccount = MaskDebitAccount(item.DebitAccount),
                    Email = item.Email,
                    ProductAmount = item.ProductAmount,
                    Fee = item.Fee,
                    TotalAmount = item.TotalAmount,
                })
                .ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int productID, CancellationToken cancellationToken)
        {
            if (productID <= 0)
            {
                return BadRequest();
            }

            var product = await _productService.GetByIdAsync(productID, cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            var model = new LikeListFormViewModel { ProductID = product.ProductID, Quantity = 1 };

            await PopulateProductsAsync(model, cancellationToken);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            LikeListFormViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid)
            {
                await PopulateProductsAsync(model, cancellationToken);

                return View(model);
            }

            var userID = User.GetRequiredUserID();

            var request = new SaveLikeListRequest
            {
                ProductID = model.ProductID,
                Quantity = model.Quantity,
            };

            try
            {
                await _likeListService.CreateAsync(userID, request, cancellationToken);
            }
            catch (InvalidProductException exception)
            {
                ModelState.AddModelError(nameof(model.ProductID), exception.Message);

                await PopulateProductsAsync(model, cancellationToken);

                return View(model);
            }

            TempData["SuccessMessage"] = "已新增喜好金融商品。";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateProductsAsync(
            LikeListFormViewModel model,
            CancellationToken cancellationToken
        )
        {
            var products = await _productService.GetListAsync(cancellationToken);

            model.Products = products
                .Select(product => new SelectListItem
                {
                    Value = product.ProductID.ToString(CultureInfo.InvariantCulture),
                    Text =
                        $"{product.ProductName} "
                        + $"（NT$ {product.Price:N2}，"
                        + $"費率 {product.FeeRate:P2}）",
                    Selected = product.ProductID == model.ProductID,
                })
                .ToList();
        }

        private static string MaskDebitAccount(string debitAccount)
        {
            if (string.IsNullOrEmpty(debitAccount))
            {
                return string.Empty;
            }

            if (debitAccount.Length <= 4)
            {
                return new string('*', debitAccount.Length);
            }

            return new string('*', debitAccount.Length - 4) + debitAccount[^4..];
        }
    }
}
