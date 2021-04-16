using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("msUoMConversions", Schema = "dbo")]
    public class UoMConversion : BaseEntity
    {
        public int Id { get; set; }

        public int UomId { get; set; }

        [Required]
        [StringLength(20)]
        public string UnitToConvert { get; set; }

        [Required]
        [StringLength(20)]
        public string UnitEquivalent { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Conversion { get; set; }

        public bool IsBaseUnit { get; set; }

        public int Seq { get; set; }
    }
}
