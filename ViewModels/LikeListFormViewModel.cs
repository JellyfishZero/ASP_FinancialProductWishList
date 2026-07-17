using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_FinancialProductWishList.ViewModels
{
    public class LikeListFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "請選擇金融商品。")]
        [Display(Name = "金融商品")]
        public int ProductID { get; set; }

        [Range(1, 1_000_000, ErrorMessage = "購買數量須介於 1 至 1,000,000。")]
        [Display(Name = "購買數量")]
        public int Quantity { get; set; } = 1;

        public IReadOnlyList<SelectListItem> Products { get; set; } = Array.Empty<SelectListItem>();
    }
}
