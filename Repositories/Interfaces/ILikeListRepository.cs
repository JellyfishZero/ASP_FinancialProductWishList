using ASP_FinancialProductWishList.Models.Entities;

namespace ASP_FinancialProductWishList.Repositories.Interfaces
{
    public interface ILikeListRepository
    {
        Task<IReadOnlyList<LikeListItem>> GetByUserIdAsync(
            long userID,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItem?> GetByIdAndUserIdAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItem> CreateAsync(
            long userID,
            int productID,
            int quantity,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItem> UpdateAsync(
            long likeListID,
            long userID,
            int productID,
            int quantity,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        );
    }
}
