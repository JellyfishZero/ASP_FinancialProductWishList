using System.ComponentModel.DataAnnotations;

namespace ASP_FinancialProductWishList.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "使用者代號")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入扣款帳號。")]
        [RegularExpression(@"^[0-9]{10,20}$", ErrorMessage = "扣款帳號須為 10 至 20 位數字。")]
        [Display(Name = "扣款帳號")]
        public string DebitAccount { get; set; } = string.Empty;
    }
}
