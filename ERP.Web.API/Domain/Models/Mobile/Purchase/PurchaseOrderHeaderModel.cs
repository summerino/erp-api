using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Web.API.Domain.Models.Mobile.Purchase
{
    public class PurchaseOrderHeaderModel
    {
        [StringLength(17)]
        public string Code { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }
        public string SupName { get; set; }
        public string SupPhone { get; set; }
        public int SupTypeId { get; set; }
        public string SupTypeName { get; set; }
        public int srcTrans { get; set; }
        public string WarehouseCode { get; set; }
        public string Mark { get; set; }
    }
}
