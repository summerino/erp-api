using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.General
{
    [Table("PaymentTerm", Schema = Schema.General)]
    public class PaymentTerm : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public short Due { get; set; }
    }

    public class VwPaymentTerm : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public short Due { get; set; }


        public string UpdatedInitial { get; set; }
    }
}
