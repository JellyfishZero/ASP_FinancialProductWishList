using System.Globalization;
using System.Security.Claims;
using ASP_FinancialProductWishList.Common.Exceptions;
using ASP_FinancialProductWishList.Services.DTOs;
using ASP_FinancialProductWishList.Services.Interfaces;
using ASP_FinancialProductWishList.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_FinancialProductWishList.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new RegisterRequest
            {
                UserName = model.UserName,
                Name = model.Name,
                Email = model.Email,
                DebitAccount = model.DebitAccount,
                Password = model.Password,
            };

            try
            {
                await _accountService.RegisterAsync(request, cancellationToken);
            }
            catch (DuplicateUserNameException exception)
            {
                ModelState.AddModelError(nameof(model.UserName), exception.Message);

                return View(model);
            }
            catch (DuplicateEmailException exception)
            {
                ModelState.AddModelError(nameof(model.Email), exception.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = "註冊成功，請使用新帳號登入。";

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new LoginRequest { UserName = model.UserName, Password = model.Password };

            var user = await _accountService.LoginAsync(request, cancellationToken);

            if (user is null)
            {
                // 不分辨帳號不存在或密碼錯誤，避免洩漏會員資訊。
                ModelState.AddModelError(string.Empty, "使用者代號或密碼不正確。");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserID.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.GivenName, user.Name),
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Product");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Product");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
