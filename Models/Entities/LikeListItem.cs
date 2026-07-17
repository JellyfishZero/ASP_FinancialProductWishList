namespace ASP_FinancialProductWishList.Models.Entities
{
    public class LikeListItem
    {
        public long LikeListID { get; set; }

        public long UserID { get; set; }

        public int ProductID { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal FeeRate { get; set; }

        public int Quantity { get; set; }

        public string DebitAccount { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
