namespace ERP.Web.API.Domain.Models.Mobile.General
{
    public class CustomerModel
    {
        public string Code { get; set; }
        public string Initial { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
        public int? AreaId1 { get; set; }
        public int? AreaId2 { get; set; }
        public int? AreaId3 { get; set; }
        public int? AreaId4 { get; set; }
        public int? AreaId5 { get; set; }
        public string AreaName1 { get; set; }
        public string AreaName2 { get; set; }
        public string AreaName3 { get; set; }
        public string AreaName4 { get; set; }
        public string AreaName5 { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string InitialAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string ContactPerson { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
