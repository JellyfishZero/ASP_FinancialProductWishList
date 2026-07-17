using ASP_FinancialProductWishList.Services.DTOs;

namespace ASP_FinancialProductWishList.Services.Interfaces
{
    public interface ILikeListService
    {
        Task<IReadOnlyList<LikeListItemResult>> GetListAsync(
            long userID,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItemResult?> GetByIdAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItemResult> CreateAsync(
            long userID,
            SaveLikeListRequest request,
            CancellationToken cancellationToken = default
        );

        Task<LikeListItemResult> UpdateAsync(
            long likeListID,
            long userID,
            SaveLikeListRequest request,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        );
    }
}
