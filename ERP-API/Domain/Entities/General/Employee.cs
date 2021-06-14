using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.General
{
    [Table("Employee", Schema = Schema.General)]
    public class Employee : BaseEntityWithActive
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        public short Type { get; set; }

        public int? SalesGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public bool Sex { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string BirthPlace { get; set; }

        public byte? MaritalStatus { get; set; }

        [StringLength(20)]
        public string IdentityCardNo { get; set; }

        public byte Religion { get; set; }

        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }
    }

    public class VwEmployee : BaseEntityWithActive
    {
        public long Id { get; set; }

        public string Initial { get; set; }

        public short Type { get; set; }

        public int? SalesGroupId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Sex { get; set; }

        public DateTime? BirthDate { get; set; }

        public string BirthPlace { get; set; }

        public byte? MaritalStatus { get; set; }

        public string IdentityCardNo { get; set; }

        public byte Religion { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Phone { get; set; }


        public string UpdatedInitial { get; set; }
    }
}
