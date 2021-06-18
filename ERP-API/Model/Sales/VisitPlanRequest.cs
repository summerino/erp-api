using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class VisitPlanRequest : VisitPlanHeader
    {
        public IEnumerable<VisitPlanDetail> ItemDetails { get; set; }

        public IEnumerable<VisitPlanDetailCustomer> CustomerDetails { get; set; }
    }
}
