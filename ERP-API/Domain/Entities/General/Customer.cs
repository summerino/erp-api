using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities.General
{
    [Table("msCustomers", Schema = "dbo")]
    public class Customer : BaseEntityWithActive
    {
        [StringLength(8)]
        [Key]
        public string Code { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Initial { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

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

        [Column(TypeName = "decimal(19, 4)")]
        public decimal CreditLimit { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [StringLength(300)]
        public string Note { get; set; }

    }

    public class VwCustomer : BaseEntityWithActive
    {
        [StringLength(8)]
        [Key]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Initial { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

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

        [Column(TypeName = "decimal(19, 4)")]
        public decimal CreditLimit { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [StringLength(300)]
        public string Note { get; set; }

        public string TypeName { get; set; }

    }
}
