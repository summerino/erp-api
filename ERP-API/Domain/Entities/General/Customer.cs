using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.General
{
    [Table("msCustomers", Schema = "dbo")]
    public class Customer : BaseEntityWithActive
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

        [StringLength(50)]
        public string Website { get; set; }

        public short CreditTerm { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditLimit { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    [Table("msCustomerTypes", Schema = "dbo")]
    public class CustomerType : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
