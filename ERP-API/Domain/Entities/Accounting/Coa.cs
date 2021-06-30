using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Accounting
{
    [Table("COA", Schema = Schema.Accounting)]
    public class Coa : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(6)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int TypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Deep { get; set; }

        [Column("LOD")]
        public byte Lod { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        [StringLength(3)]
        public string CurrCode { get; set; }

        [StringLength(1)]
        public string CBType { get; set; }

        [StringLength(4)]
        public string VouCode { get; set; }

        [StringLength(4)]
        public string BsCode { get; set; }

        [StringLength(4)]
        public string IsCode { get; set; }

        [StringLength(4)]
        public string IsDetCode { get; set; }
    }

    [Table("COAType", Schema = Schema.Accounting)]
    public class CoaType : BaseEntityWithActive
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
