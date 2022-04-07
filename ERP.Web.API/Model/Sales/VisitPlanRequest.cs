using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class VisitPlanRequest : VisitPlanHeader
{
    public IEnumerable<VisitPlanDetail> ItemDetails { get; set; }

    public IEnumerable<VisitPlanDetailCustomer> CustomerDetails { get; set; }
}