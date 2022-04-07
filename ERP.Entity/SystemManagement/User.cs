using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.SystemManagement;

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

    public bool MobileSignIn { get; set; }
        
    public bool IsLoggedIn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLogin { get; set; }

    [StringLength(50)]
    public string SessionId { get; set; }
        
    public string TokenId { get; set; }

    [Column("IPAddress")]
    [StringLength(40)]
    public string IpAddress { get; set; }

    public bool IsMobileLoggedIn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? MobileLastLogin { get; set; }

    [StringLength(50)]
    public string MobileSessionId { get; set; }

    public string MobileTokenId { get; set; }

    [Column("MobileIPAddress")]
    [StringLength(40)]
    public string MobileIpAddress { get; set; }

    public string FirebaseTokenId { get; set; }
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

    public bool MobileSignIn { get; set; }

    public bool IsLoggedIn { get; set; }

    public DateTime? LastLogin { get; set; }

    public string SessionId { get; set; }

    public string TokenId { get; set; }

    public string IpAddress { get; set; }

    public bool IsMobileLoggedIn { get; set; }

    public DateTime? MobileLastLogin { get; set; }

    public string MobileSessionId { get; set; }

    public string MobileTokenId { get; set; }

    public string MobileIpAddress { get; set; }

    public string FirebaseTokenId { get; set; }


    public string RoleName { get; set; }

    public string EmployeeInitial { get; set; }

    public string UpdatedInitial { get; set; }
}