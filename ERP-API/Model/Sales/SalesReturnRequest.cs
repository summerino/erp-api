using ERP.Entity.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Model.Sales
{
    public class SalesReturnRequest : SalesReturnHeader
    {
        public IEnumerable<SalesReturnDetail> ItemDetails { get; set; }

        public IEnumerable<SalesReturnDetailExchDiffItem> DiffItemDetails { get; set; }

    }
}
