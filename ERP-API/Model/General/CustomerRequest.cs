using System.Collections.Generic;
using ERP.Entity.General;

namespace ERP.Web.API.Model.General
{
    public class CustomerRequest : Customer
    {
        public IEnumerable<CustomerAddress> ItemDetails { get; set; }
    }
}
