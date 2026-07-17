using ASP_FinancialProductWishList.Common.Exceptions;
using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;
using ASP_FinancialProductWishList.Services.DTOs;
using ASP_FinancialProductWishList.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace ASP_FinancialProductWishList.Services.Implementations
{
    public class LikeListService : ILikeListService
    {
        private readonly ILikeListRepository _likeListRepository;

        public LikeListService(ILikeListRepository likeListRepository)
        {
            _likeListRepository = likeListRepository;
        }

        public async Task<IReadOnlyList<LikeListItemResult>> GetListAsync(
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            ValidateUserID(userID);

            var items = await _likeListRepository.GetByUserIdAsync(userID, cancellationToken);

            return items.Select(MapResult).ToList();
        }

        public async Task<LikeListItemResult?> GetByIdAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            ValidateLikeListID(likeListID);
            ValidateUserID(userID);

            var item = await _likeListRepository.GetByIdAndUserIdAsync(
                likeListID,
                userID,
                cancellationToken
            );

            return item is null ? null : MapResult(item);
        }

        public async Task<LikeListItemResult> CreateAsync(
            long userID,
            SaveLikeListRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ValidateUserID(userID);
            ValidateRequest(request);

            try
            {
                var item = await _likeListRepository.CreateAsync(
                    userID,
                    request.ProductID,
                    request.DebitAccount.Trim(),
                    request.Quantity,
                    cancellationToken
                );

                return MapResult(item);
            }
            catch (SqlException exception) when (exception.Number == 51012)
            {
                throw new InvalidProductException(exception);
            }
        }

        public async Task<LikeListItemResult> UpdateAsync(
            long likeListID,
            long userID,
            SaveLikeListRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ValidateLikeListID(likeListID);
            ValidateUserID(userID);
            ValidateRequest(request);

            try
            {
                var item = await _likeListRepository.UpdateAsync(
                    likeListID,
                    userID,
                    request.ProductID,
                    request.DebitAccount.Trim(),
                    request.Quantity,
                    cancellationToken
                );

                return MapResult(item);
            }
            catch (SqlException exception) when (exception.Number == 51012)
            {
                throw new InvalidProductException(exception);
            }
            catch (SqlException exception) when (exception.Number == 51013)
            {
                throw new LikeListItemNotFoundException(exception);
            }
        }

        public async Task DeleteAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            ValidateLikeListID(likeListID);
            ValidateUserID(userID);

            try
            {
                await _likeListRepository.DeleteAsync(likeListID, userID, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 51013)
            {
                throw new LikeListItemNotFoundException(exception);
            }
        }

        private static LikeListItemResult MapResult(LikeListItem item)
        {
            var rawProductAmount = item.Price * item.Quantity;

            var productAmount = RoundCurrency(rawProductAmount);

            var fee = RoundCurrency(rawProductAmount * item.FeeRate);

            var totalAmount = productAmount + fee;

            return new LikeListItemResult
            {
                LikeListID = item.LikeListID,
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                Price = item.Price,
                FeeRate = item.FeeRate,
                Quantity = item.Quantity,
                DebitAccount = item.DebitAccount,
                Email = item.Email,
                ProductAmount = productAmount,
                Fee = fee,
                TotalAmount = totalAmount,
            };
        }

        private static decimal RoundCurrency(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static void ValidateRequest(SaveLikeListRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.ProductID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.ProductID),
                    "金融商品 ID 必須大於 0。"
                );
            }

            if (request.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.Quantity),
                    "購買數量必須大於 0。"
                );
            }

            var debitAccount = request.DebitAccount.Trim();

            if (
                debitAccount.Length is < 10 or > 20
                || debitAccount.Any(character => character is < '0' or > '9')
            )
            {
                throw new ArgumentException(
                    "扣款帳號須為 10 至 20 位數字。",
                    nameof(request.DebitAccount)
                );
            }
        }

        private static void ValidateUserID(long userID)
        {
            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userID), "使用者 ID 必須大於 0。");
            }
        }

        private static void ValidateLikeListID(long likeListID)
        {
            if (likeListID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(likeListID),
                    "喜好項目 ID 必須大於 0。"
                );
            }
        }
    }
}
