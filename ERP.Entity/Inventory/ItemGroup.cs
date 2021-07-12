using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory
{
    [Table("ItemGroup", Schema = Schema.Inventory)]
    public class ItemGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Initial { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }

    [Table("ItemGroupSubGroup", Schema = Schema.Inventory)]
    public class ItemGroupSubGroup
    {
        public int Id { get; set; }

        public int ItemGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Value { get; set; }
    }

    public class VwItemGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }
}
