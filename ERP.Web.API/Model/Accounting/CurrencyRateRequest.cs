using ERP.Entity.Accounting;

namespace ERP.Web.API.Model.Accounting
{
    public class CurrencyRateRequest : CurrencyRate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
