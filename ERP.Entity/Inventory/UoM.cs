using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory;

[Table("UoM", Schema = Schema.Inventory)]
public class UoM : BaseEntityWithActive
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Initial { get; set; }

    [StringLength(100)]
    public string Description { get; set; }

    [Required]
    [StringLength(20)]
    public string BaseUnit { get; set; }
}

public class VwUoM : BaseEntityWithActive
{
    public int Id { get; set; }

    public string Initial { get; set; }

    public string Description { get; set; }

    public string BaseUnit { get; set; }


    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }
}