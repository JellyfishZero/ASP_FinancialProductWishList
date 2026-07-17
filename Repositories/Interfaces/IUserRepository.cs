using ASP_FinancialProductWishList.Models.Entities;

namespace ASP_FinancialProductWishList.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<(bool UserNameExists, bool EmailExists)> ExistsByUserNameOrEmailAsync(
            string userName,
            string email,
            CancellationToken cancellationToken = default
        );

        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

        Task<User?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        );

        Task<User?> GetByIdAsync(long userID, CancellationToken cancellationToken = default);

        Task<User> UpdateDebitAccountAsync(
            long userID,
            string debitAccount,
            CancellationToken cancellationToken = default
        );
    }
}
