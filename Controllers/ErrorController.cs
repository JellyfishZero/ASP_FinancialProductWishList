using ASP_FinancialProductWishList.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_FinancialProductWishList.Controllers
{
    public sealed class ErrorController : Controller
    {
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Index()
        {
            return View(
                "Error",
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                }
            );
        }
    }
}
