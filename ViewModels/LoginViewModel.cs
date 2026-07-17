using System.ComponentModel.DataAnnotations;

namespace ASP_FinancialProductWishList.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入使用者代號。")]
        [StringLength(50, ErrorMessage = "使用者代號不可超過 50 個字元。")]
        [Display(Name = "使用者代號")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼。")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
