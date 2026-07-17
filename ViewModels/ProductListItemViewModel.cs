namespace ASP_FinancialProductWishList.ViewModels
{
    public class ProductListItemViewModel
    {
        public int ProductID { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public decimal FeeRate { get; init; }
    }
}
