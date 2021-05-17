using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("Area", Schema = Schema.Sales)]
    public class Area : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Initial { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int? Deep { get; set; }

        public string Lineage { get; set; }
    }

    public class VwArea : BaseEntityWithActive
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Initial { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int? Deep { get; set; }

        public string Lineage { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }
}
