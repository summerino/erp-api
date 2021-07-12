using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.General
{
    [Table("Supplier", Schema = Schema.General)]
    public class Supplier : BaseEntityWithActive
    {
        [Key]
        [StringLength(8)]
        public string Code { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int TypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }
    }

    public class VwSupplier : BaseEntityWithActive
    {
        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string RefNo { get; set; }


        public string TypeName { get; set; }

        public string UpdatedInitial { get; set; }
    }

    [Table("SupplierType", Schema = Schema.General)]
    public class SupplierType : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    public class VwSupplierType : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }


        public string UpdatedInitial { get; set; }
    }
}
