using System;
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
        //public long RequestBy { get; set; }
        //[StringLength(8)]
        //public string WarehouseCode { get; set; }
        public string SupName { get; set; }
        public string SupPhone { get; set; }
        //public string Status/Mark { get; set; }
    }
}
