using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("User", Schema = Schema.SystemManagement)]
    public class User : BaseEntityWithActive
    {
        public int Id { get; set; }

        public Guid CatalogUserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int RoleId { get; set; }

        public long? EmployeeId { get; set; }

        public bool IsLoggedIn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastLogin { get; set; }

        [StringLength(50)]
        public string SessionId { get; set; }
        
        public string TokenId { get; set; }

        [Column("IPAddress")]
        [StringLength(40)]
        public string IpAddress { get; set; }
    }

    public class VwUser : BaseEntityWithActive
    {
        public int Id { get; set; }

        public Guid CatalogUserId { get; set; }

        public string Username { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int RoleId { get; set; }

        public long? EmployeeId { get; set; }

        public bool IsLoggedIn { get; set; }

        public DateTime? LastLogin { get; set; }

        public string SessionId { get; set; }

        public string TokenId { get; set; }

        public string IpAddress { get; set; }


        public string RoleName { get; set; }

        public string EmployeeUsername { get; set; }

        public string UpdatedInitial { get; set; }
    }

    public class VMUser : BaseEntityWithActive
    {
        public int Id { get; set; }

        public Guid CatalogUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int RoleId { get; set; }

        public long? EmployeeId { get; set; }

        public string Password { get; set; }

        public bool IsLoggedIn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastLogin { get; set; }

        [StringLength(50)]
        public string SessionId { get; set; }

        public string TokenId { get; set; }

        [Column("IPAddress")]
        [StringLength(40)]
        public string IpAddress { get; set; }
    }
}
