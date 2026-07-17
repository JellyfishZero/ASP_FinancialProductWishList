using System.Globalization;
using System.Security.Claims;

namespace ASP_FinancialProductWishList.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static long GetRequiredUserID(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal);

            var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (
                !long.TryParse(
                    value,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var userID
                )
                || userID <= 0
            )
            {
                throw new UnauthorizedAccessException("登入資訊缺少有效的使用者 ID。");
            }

            return userID;
        }
    }
}
