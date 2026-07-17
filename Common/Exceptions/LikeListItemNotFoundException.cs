namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public class LikeListItemNotFoundException : Exception
    {
        public LikeListItemNotFoundException(Exception innerException)
            : base("找不到指定的喜好項目。", innerException) { }
    }
}
