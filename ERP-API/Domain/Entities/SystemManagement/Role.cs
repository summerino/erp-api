using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("Role", Schema = Schema.SystemManagement)]
    public class Role : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    [Table("RoleMenuAction", Schema = Schema.SystemManagement)]
    [Index(nameof(RoleId), nameof(MenuId), nameof(ActionId), IsUnique = true)]
    public class RoleMenuAction
    {
        public long Id { get; set; }

        public int RoleId { get; set; }

        public int MenuId { get; set; }

        public int ActionId { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }
    }
}
