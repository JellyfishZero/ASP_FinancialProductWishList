namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public class DuplicateLikeListItemException : Exception
    {
        public DuplicateLikeListItemException(Exception innerException)
            : base("此金融商品已存在於喜好清單。", innerException) { }
    }
}
