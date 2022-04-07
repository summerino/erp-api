using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.NetRevenue;

public class NetRevenueDetailByCoaModel
{
    public string Code { get; set; }
    public DateTime Date { get; set; }
    [Required]
    public string CustCode { get; set; }
    [Required]
    public string CustName { get; set; }
    [Required]
    public string CoaCode { get; set; }
    [Required]
    public string TransCode { get; set; }
    public decimal Amount { get; set; }
    [Required]
    public string SrcTrans { get; set; }
}