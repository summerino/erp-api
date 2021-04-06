using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("msItemCategories", Schema = "dbo")]
    public class ItemCategory : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public bool IsLowestLevel { get; set; }

        [StringLength(50)]
        public string GroupId { get; set; }

        public int? Deep { get; set; }

        public int? Seq { get; set; }

        public string Lineage { get; set; }
    }
}
