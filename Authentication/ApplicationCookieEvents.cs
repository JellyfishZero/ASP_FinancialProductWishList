using System.Globalization;
using System.Security.Claims;
using ASP_FinancialProductWishList.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ASP_FinancialProductWishList.Authentication
{
    public sealed class ApplicationCookieEvents
        : CookieAuthenticationEvents
    {
        private readonly IAccountService _accountService;

        public ApplicationCookieEvents(
            IAccountService accountService
        )
        {
            _accountService = accountService;
        }

        public override async Task ValidatePrincipal(
            CookieValidatePrincipalContext context
        )
        {
            var userIDValue = context.Principal?
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            var userName = context.Principal?
                .FindFirst(ClaimTypes.Name)
                ?.Value;

            if (
                !long.TryParse(
                    userIDValue,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var userID
                ) ||
                userID <= 0 ||
                string.IsNullOrWhiteSpace(userName)
            )
            {
                await RejectPrincipalAsync(context);
                return;
            }

            var user = await _accountService.GetProfileAsync(
                userID,
                context.HttpContext.RequestAborted
            );

            if (
                user is null ||
                !string.Equals(
                    user.UserName,
                    userName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                await RejectPrincipalAsync(context);
            }
        }

        private static async Task RejectPrincipalAsync(
            CookieValidatePrincipalContext context
        )
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }
    }
}