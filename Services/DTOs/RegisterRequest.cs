namespace ASP_FinancialProductWishList.Services.DTOs
{
    public class RegisterRequest
    {
        public string UserName { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string DebitAccount { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}
