using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_FinancialProductWishList.ViewModels
{
    public class LikeListFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "請選擇金融商品。")]
        [Display(Name = "金融商品")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "請輸入扣款帳號。")]
        [RegularExpression(@"^[0-9]{10,20}$", ErrorMessage = "扣款帳號須為 10 至 20 位數字。")]
        [Display(Name = "預計扣款帳號")]
        public string DebitAccount { get; set; } = string.Empty;

        [Range(1, 1_000_000, ErrorMessage = "購買數量須介於 1 至 1,000,000。")]
        [Display(Name = "購買數量")]
        public int Quantity { get; set; } = 1;

        public IReadOnlyList<SelectListItem> Products { get; set; } = Array.Empty<SelectListItem>();
    }
}
