using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.SystemManagement;

[Table("SystemParameter", Schema = Schema.SystemManagement)]
[Index(nameof(Code), IsUnique = true)]
public class SystemParameter
{
    public int Id { get; set; }

    public int ModuleId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; }

    [Required]
    [StringLength(100)]
    public string Value { get; set; }

    [Required]
    [StringLength(20)]
    public string DataType { get; set; }

    [Required]
    [StringLength(256)]
    public string Description { get; set; }

    public int Seq { get; set; }

    public bool IsActive { get; set; }
}

[Table("SystemParameterModule", Schema = Schema.SystemManagement)]
public class SystemParameterModule
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int? ParentId { get; set; }

    public int Deep { get; set; }

    public int Seq { get; set; }

    public bool IsActive { get; set; }
}