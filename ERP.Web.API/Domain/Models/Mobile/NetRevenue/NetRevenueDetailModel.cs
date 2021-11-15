using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.NetRevenue
{
    public class NetRevenueDetailModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public string CoaCode { get; set; }
        [Required]
        public string CoaName { get; set; }
        public decimal Amount { get; set; }
    }
}
