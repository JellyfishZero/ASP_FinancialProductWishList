namespace ASP_FinancialProductWishList.Common.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException()
            : base("此 Email 已被使用。") { }

        public DuplicateEmailException(Exception innerException)
            : base("此 Email 已被使用。", innerException) { }
    }
}
