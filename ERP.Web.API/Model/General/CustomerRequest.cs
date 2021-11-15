using ERP.Entity.General;

namespace ERP.Web.API.Model.General
{
    public class CustomerRequest : Customer
    {
        public string MobilePassword { get; set; }
        public IEnumerable<CustomerAddress> ItemDetails { get; set; }
    }
}
