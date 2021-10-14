using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.ItemRequest
{
    public class ItemRequestDetail
    {
        [Required]
        public int ItemId { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public int LineNo { get; set; }
        [Required]
        public int UnitId { get; set; }
        [Required]
        public string UnitName { get; set; }
        [Required]
        public decimal Quantity { get; set; }
    }
}
