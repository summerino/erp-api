using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileCustomer", Schema = Schema.MobileSales)]
    public class MobileCustomer : BaseEntityWithApproved
    {
        [Key]
        [StringLength(8)]
        public string Code { get; set; }

        [StringLength(8)]
        public string CustCode { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int TypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string InitialAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(50)]
        public string ContactPerson { get; set; }

        [Required]
        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }
    }
}
