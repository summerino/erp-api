using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("Company", Schema = Schema.SystemManagement)]
    public class Company
    {
        public short Id { get; set; }

        public int CatalogTenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(40)]
        public string City { get; set; }

        [StringLength(10)]
        public string Zip { get; set; }

        [Required]
        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [StringLength(20)]
        [Column("NPWP")]
        public string Npwp { get; set; }

        [Column("PKPDate", TypeName = "date")]
        public DateTime? PkpDate { get; set; }

        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }
    }
}
