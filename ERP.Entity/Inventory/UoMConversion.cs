using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory;

[Table("UoMConversion", Schema = Schema.Inventory)]
public class UoMConversion
{
    public int Id { get; set; }

    public int UomId { get; set; }

    [Required]
    [StringLength(20)]
    public string UnitToConvert { get; set; }

    [Required]
    [StringLength(20)]
    public string UnitEquivalent { get; set; }

    [Precision(19, 6)]
    public decimal Conversion { get; set; }

    public bool IsBaseUnit { get; set; }

    public int Seq { get; set; }
}