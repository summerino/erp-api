using ERP.Entity.Accounting;

namespace ERP.Web.API.Model.Accounting
{
    public class ClosingMonthRequest : ClosingMonth
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
