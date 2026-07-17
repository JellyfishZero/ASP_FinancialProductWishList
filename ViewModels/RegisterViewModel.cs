using System.ComponentModel.DataAnnotations;

namespace ASP_FinancialProductWishList.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入使用者代號。")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "使用者代號長度須為 4 至 50 個字元。")]
        [RegularExpression(
            @"^[A-Za-z0-9_]+$",
            ErrorMessage = "使用者代號只能包含英文字母、數字與底線。"
        )]
        [Display(Name = "使用者代號")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入姓名。")]
        [StringLength(100, ErrorMessage = "姓名不可超過 100 個字元。")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入 Email。")]
        [EmailAddress(ErrorMessage = "Email 格式不正確。")]
        [StringLength(254, ErrorMessage = "Email 不可超過 254 個字元。")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入扣款帳號。")]
        [RegularExpression(@"^[0-9]{10,20}$", ErrorMessage = "扣款帳號須為 10 至 20 位數字。")]
        [Display(Name = "扣款帳號")]
        public string DebitAccount { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼。")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度須為 8 至 100 個字元。")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請再次輸入密碼。")]
        [Compare(nameof(Password), ErrorMessage = "兩次輸入的密碼不一致。")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
