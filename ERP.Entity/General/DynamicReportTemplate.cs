using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.General
{
    [Table("DynamicReportTemplate", Schema = Schema.General)]
    public class DynamicReportTemplate : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string DataTypeParameter1 { get; set; }

        [StringLength(50)]
        public string SourceParameter1 { get; set; }

        [StringLength(100)]
        public string DropdownValueParameter1 { get; set; }

        [StringLength(256)]
        public string DropdownTextParameter1 { get; set; }

        [StringLength(20)]
        public string DataTypeParameter2 { get; set; }

        [StringLength(50)]
        public string SourceParameter2 { get; set; }

        [StringLength(100)]
        public string DropdownValueParameter2 { get; set; }

        [StringLength(256)]
        public string DropdownTextParameter2 { get; set; }

        [StringLength(20)]
        public string DataTypeParameter3 { get; set; }

        [StringLength(50)]
        public string SourceParameter3 { get; set; }

        [StringLength(100)]
        public string DropdownValueParameter3 { get; set; }

        [StringLength(256)]
        public string DropdownTextParameter3 { get; set; }

        [StringLength(20)]
        public string DataTypeParameter4 { get; set; }

        [StringLength(50)]
        public string SourceParameter4 { get; set; }

        [StringLength(100)]
        public string DropdownValueParameter4 { get; set; }

        [StringLength(256)]
        public string DropdownTextParameter4 { get; set; }

        [StringLength(20)]
        public string DataTypeParameter5 { get; set; }

        [StringLength(50)]
        public string SourceParameter5 { get; set; }

        [StringLength(100)]
        public string DropdownValueParameter5 { get; set; }

        [StringLength(256)]
        public string DropdownTextParameter5 { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Query { get; set; }
    }
}
