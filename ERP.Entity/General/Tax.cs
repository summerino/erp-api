using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.General;

[Table("Tax", Schema = Schema.General)]
[Index(nameof(CoaCode))]
public class Tax : BaseEntityWithActive
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Initial { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public short TypeId { get; set; }

    [Precision(5, 2)]
    public decimal Rate { get; set; }

    [Precision(5, 2)]
    public decimal ExemptRate { get; set; }

    [StringLength(6)]
    public string CoaCode { get; set; }

    [StringLength(6)]
    public string ExemptCoaCode { get; set; }

    public short Seq { get; set; }
}

public class VwTax : BaseEntityWithActive
{
    public int Id { get; set; }

    public string Initial { get; set; }

    public string Name { get; set; }

    public short TypeId { get; set; }

    [Precision(5, 2)]
    public decimal Rate { get; set; }

    [Precision(5, 2)]
    public decimal ExemptRate { get; set; }

    public string CoaCode { get; set; }

    public string ExemptCoaCode { get; set; }

    public short Seq { get; set; }


    public string CoaName { get; set; }

    public string UpdatedInitial { get; set; }
}