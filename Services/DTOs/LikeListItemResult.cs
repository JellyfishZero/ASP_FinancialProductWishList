namespace ASP_FinancialProductWishList.Services.DTOs
{
    public class LikeListItemResult
    {
        public long LikeListID { get; init; }

        public int ProductID { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public decimal FeeRate { get; init; }

        public int Quantity { get; init; }

        public string DebitAccount { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public decimal ProductAmount { get; init; }

        public decimal Fee { get; init; }

        public decimal TotalAmount { get; init; }
    }
}
