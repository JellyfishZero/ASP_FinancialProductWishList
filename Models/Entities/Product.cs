namespace ASP_FinancialProductWishList.Models.Entities
{
    public class Product
    {
        public int ProductID { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public decimal FeeRate { get; init; }
    }
}
