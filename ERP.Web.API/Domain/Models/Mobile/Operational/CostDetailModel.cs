namespace ERP.Web.API.Domain.Models.Mobile.Operational;

public class CostDetailModel
{
    //public long Id { get; set; }
    public string Code { get; set; }
    public short LineNo { get; set; }
    public string CoaCode { get; set; }
    public string CoaName { get; set; }
    public decimal Amount { get; set; }
}