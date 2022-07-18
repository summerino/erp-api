using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.SystemManagement;

[Table("Menu", Schema = Schema.SystemManagement)]
public class Menu : BaseEntityWithActive
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public int? ParentId { get; set; }

    public int Deep { get; set; }

    public int Seq { get; set; }

    [StringLength(100)]
    public string Link { get; set; }

    [StringLength(50)]
    public string Icon { get; set; }

    [StringLength(100)]
    public string Regex { get; set; }
}

[Table("MenuAction", Schema = Schema.SystemManagement)]
[Index(nameof(MenuId), nameof(ActionId), IsUnique = true)]
public class MenuAction
{
    public long Id { get; set; }

    public int MenuId { get; set; }

    public int ActionId { get; set; }

    public int Seq { get; set; }
}