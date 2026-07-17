namespace ASP_FinancialProductWishList.Services.DTOs
{
    public class SaveLikeListRequest
    {
        public int ProductID { get; init; }

        public string DebitAccount { get; init; } = string.Empty;

        public int Quantity { get; init; }
    }
}
