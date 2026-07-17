namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public sealed class UserNotFoundException : Exception
    {
        public UserNotFoundException(Exception innerException)
            : base("找不到指定的使用者。", innerException) { }
    }
}
