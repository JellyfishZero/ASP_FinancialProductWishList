namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public class DuplicateUserNameException : Exception
    {
        public DuplicateUserNameException()
            : base("此使用者代號已被使用。") { }

        public DuplicateUserNameException(Exception innerException)
            : base("此使用者代號已被使用。", innerException) { }
    }
}
