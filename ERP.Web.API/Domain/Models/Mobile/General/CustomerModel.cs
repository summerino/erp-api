namespace ERP.Web.API.Domain.Models.Mobile.General
{
    public class CustomerModel
    {
        public String Code { get; set; }
        public String Initial { get; set; }
        public String Name { get; set; }
        public int TypeId { get; set; }
        public String TypeName { get; set; }
        public String Email { get; set; }
        public String Website { get; set; }
        public int PaymentTermId { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
        public String RefNo { get; set; }
        public String Notes { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }
        public int? AreaId1 { get; set; }
        public int? AreaId2 { get; set; }
        public int? AreaId3 { get; set; }
        public int? AreaId4 { get; set; }
        public int? AreaId5 { get; set; }
        public String AreaName1 { get; set; }
        public String AreaName2 { get; set; }
        public String AreaName3 { get; set; }
        public String AreaName4 { get; set; }
        public String AreaName5 { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string InitialAddress { get; set; }
        public String Address1 { get; set; }
        public String Address2 { get; set; }
        public String Phone { get; set; }
        public String Fax { get; set; }
        public String ContactPerson { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsConsignee { get; set; }
        public bool MobileSignIn { get; set; }
    }
}
