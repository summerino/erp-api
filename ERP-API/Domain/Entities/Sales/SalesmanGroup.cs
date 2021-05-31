using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("SalesmanGroup", Schema = Schema.Sales)]
    public class SalesmanGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public long SupervisorId { get; set; }
    }

    public class VwSalesmanGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public long SupervisorId { get; set; }


        public string SupervisorInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }
}
