using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting
{
    [Table("COA", Schema = Schema.Accounting)]
    [Index(nameof(Code), IsUnique = true)]
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

    public class VwCoa : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public int? ParentId { get; set; }

        public int? Deep { get; set; }

        public string Description { get; set; }

        public string CurrCode { get; set; }

        public string CBType { get; set; }

        public string VouCode { get; set; }

        public string BsCode { get; set; }

        public string IsCode { get; set; }

        public string IsDetCode { get; set; }

        public string TypeName { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public int IsParent { get; set; }
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
