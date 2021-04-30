using System.Collections.Generic;
using ERP_API.Domain.Entities.General;

namespace ERP_API.Model.General
{
    public class CustomerRequest : Customer
    {
        public IEnumerable<CustomerAddress> ItemDetails { get; set; }
    }
}
