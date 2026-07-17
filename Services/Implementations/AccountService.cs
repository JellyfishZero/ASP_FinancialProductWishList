using ASP_FinancialProductWishList.Common.Exceptions;
using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;
using ASP_FinancialProductWishList.Services.DTOs;
using ASP_FinancialProductWishList.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace ASP_FinancialProductWishList.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var userName = request.UserName.Trim();
            var name = request.Name.Trim();
            var email = request.Email.Trim();
            var debitAccount = request.DebitAccount.Trim();

            var existence = await _userRepository.ExistsByUserNameOrEmailAsync(
                userName,
                email,
                cancellationToken
            );

            if (existence.UserNameExists)
            {
                throw new DuplicateUserNameException();
            }

            if (existence.EmailExists)
            {
                throw new DuplicateEmailException();
            }

            var user = new User
            {
                UserName = userName,
                Name = name,
                Email = email,
                DebitAccount = debitAccount,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            try
            {
                return await _userRepository.CreateAsync(user, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 51001)
            {
                // 防止「完成重複檢查後、執行 INSERT 前」
                // 另一個請求剛好新增相同資料的競爭狀況。
                var latestExistence = await _userRepository.ExistsByUserNameOrEmailAsync(
                    userName,
                    email,
                    cancellationToken
                );

                if (latestExistence.UserNameExists)
                {
                    throw new DuplicateUserNameException(exception);
                }

                if (latestExistence.EmailExists)
                {
                    throw new DuplicateEmailException(exception);
                }

                throw;
            }
        }

        public async Task<User?> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var userName = request.UserName.Trim();

            var user = await _userRepository.GetByUserNameAsync(userName, cancellationToken);

            if (user is null)
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // 登入完成後，不讓 PasswordHash 繼續往 Controller 流動。
            user.PasswordHash = string.Empty;

            return user;
        }

        public Task<User?> GetProfileAsync(
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userID), "使用者 ID 必須大於 0。");
            }

            return _userRepository.GetByIdAsync(userID, cancellationToken);
        }

        public async Task<User> UpdateDebitAccountAsync(
            long userID,
            UpdateDebitAccountRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userID), "使用者 ID 必須大於 0。");
            }

            ArgumentNullException.ThrowIfNull(request);

            var debitAccount = request.DebitAccount.Trim();

            if (
                debitAccount.Length is < 10 or > 20
                || debitAccount.Any(character => character is < '0' or > '9')
            )
            {
                throw new ArgumentException("扣款帳號須為 10 至 20 位數字。", nameof(request));
            }

            try
            {
                return await _userRepository.UpdateDebitAccountAsync(
                    userID,
                    debitAccount,
                    cancellationToken
                );
            }
            catch (SqlException exception) when (exception.Number == 51002)
            {
                throw new UserNotFoundException(exception);
            }
        }
    }
}
