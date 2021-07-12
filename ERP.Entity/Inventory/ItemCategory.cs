using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory
{
    [Table("ItemCategory", Schema = Schema.Inventory)]
    public class ItemCategory : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Initial { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int? GroupId { get; set; }

        public int? Deep { get; set; }

        public int? Seq { get; set; }

        public string Lineage { get; set; }
    }

    public class VwItemCategory : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int? GroupId { get; set; }

        public int? Deep { get; set; }

        public int? Seq { get; set; }

        public string Lineage { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }
}
