using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory;

[Table("Warehouse", Schema = Schema.Inventory)]
public class Warehouse : BaseEntityWithActive
{
    [Key]
    [StringLength(8)]
    public string Code { get; set; }

    [Required]
    [StringLength(20)]
    public string Initial { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(30)]
    public string Phone { get; set; }

    public bool IsDefault { get; set; }

    [StringLength(8)]
    public string CustCode { get; set; }
}

public class VwWarehouse : BaseEntityWithActive
{
    public string Code { get; set; }

    public string Initial { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public string Phone { get; set; }

    public bool IsDefault { get; set; }

    public string CustCode { get; set; }


    public string UpdatedInitial { get; set; }
}