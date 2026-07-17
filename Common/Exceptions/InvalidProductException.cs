namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public class InvalidProductException : Exception
    {
        public InvalidProductException(Exception innerException)
            : base("指定的金融商品不存在。", innerException) { }
    }
}
