namespace ASP_FinancialProductWishList.Models.Entities
{
    public class User
    {
        public long UserID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string DebitAccount { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
}
