using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.Operational;

public class CostRequestHeader
{
    [Required]
    public string Code { get; set; }
    [Required]
    public string CashBankCode { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public long SalesmanId { get; set; }
    [Required]
    public string CurrencyCode { get; set; }
    [Required]
    public decimal Rate { get; set; }
    [Required]
    public decimal Total { get; set; }
    [Required]
    public string Mark { get; set; }
}