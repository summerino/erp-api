namespace ERP.Web.API.Domain.Models.Mobile.Sales;

public class SalesProfile
{
    public int UserId { get; set; }
    public long SalesId { get; set; }
    public string SalesInitialId { get; set; }
    public string SalesName { get; set; }
    public string SalesGroup { get; set; }
    public int TaxInclude { get; set; }
    public int ShowTaxOption { get; set; }
}