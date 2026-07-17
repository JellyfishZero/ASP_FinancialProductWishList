using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Services.DTOs;

namespace ASP_FinancialProductWishList.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default
        );

        Task<User?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
