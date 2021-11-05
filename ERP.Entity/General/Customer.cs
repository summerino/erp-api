using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.General
{
    [Table("Customer", Schema = Schema.General)]
    public class Customer : BaseEntityWithActive
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

        public int TypeId { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Website { get; set; }

        public int PaymentTermId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditLimit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditUsed { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        public int? BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }

        public bool IsConsignee { get; set; }

        public bool MobileSignIn { get; set; }

        public Guid? CatalogUserId { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string MobileUsername { get; set; }

        public bool IsMobileLoggedIn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? MobileLastLogin { get; set; }

        [StringLength(50)]
        public string MobileSessionId { get; set; }

        public string MobileTokenId { get; set; }

        [Column("MobileIPAddress")]
        [StringLength(40)]
        public string MobileIpAddress { get; set; }
    }

    public class VwCustomer : BaseEntityWithActive
    {
        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string InitialAddress { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string ContactPerson { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public int PaymentTermId { get; set; }

        public decimal CreditLimit { get; set; }

        public decimal CreditUsed { get; set; }

        public string RefNo { get; set; }

        public string Notes { get; set; }

        public int? BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }
        
        public int? AreaId4 { get; set; }
        
        public int? AreaId5 { get; set; }

        public bool IsConsignee { get; set; }

        public bool MobileSignIn { get; set; }

        public Guid? CatalogUserId { get; set; }

        public string MobileUsername { get; set; }

        public bool IsMobileLoggedIn { get; set; }

        public DateTime? MobileLastLogin { get; set; }

        public string MobileSessionId { get; set; }

        public string MobileTokenId { get; set; }

        public string MobileIpAddress { get; set; }


        public string TypeName { get; set; }

        public string UpdatedInitial { get; set; }

        public string AreaName1 { get; set; }

        public string AreaName2 { get; set; }

        public string AreaName3 { get; set; }

        public string AreaName4 { get; set; }

        public string AreaName5 { get; set; }
    }

    [Table("CustomerType", Schema = Schema.General)]
    public class CustomerType : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    public class VwCustomerType : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }


        public string UpdatedInitial { get; set; }
    }

    [Table("CustomerAddress", Schema = Schema.General)]
    public class CustomerAddress
    {
        public int Id { get; set; }

        [StringLength(8)]
        public string Code { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(50)]
        public string ContactPerson { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Lat { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Lng { get; set; }

        public bool IsDefault { get; set; }
    }
}
