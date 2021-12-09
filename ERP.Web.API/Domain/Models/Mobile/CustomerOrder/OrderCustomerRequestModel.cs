using ERP.Entity.MobileCustomer;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;

namespace ERP.Web.API.Domain.Models.Mobile.CustomerOrder
{
    public class OrderCustomerRequestModel
    {
        public MobileOrderHeader Header { get; set; }
        public IEnumerable<VisitOrderDetailRequest> Details { get; set; }
    }
}
