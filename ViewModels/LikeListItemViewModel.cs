namespace ASP_FinancialProductWishList.ViewModels
{
    public class LikeListItemViewModel
    {
        public long LikeListID { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public decimal FeeRate { get; init; }

        public int Quantity { get; init; }

        public string MaskedDebitAccount { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public decimal ProductAmount { get; init; }

        public decimal Fee { get; init; }

        public decimal TotalAmount { get; init; }
    }
}
