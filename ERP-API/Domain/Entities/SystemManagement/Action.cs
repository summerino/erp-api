using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("Action", Schema = Schema.SystemManagement)]
    [Index(nameof(Name), IsUnique = true)]
    public class Action
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
