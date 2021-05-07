using System;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesOrderRequest : SalesOrderHeader
    {
        public IEnumerable<SalesOrderDetail> ItemDetails { get; set; }

        public DateTime DlvDate { get; set; }

        public bool IsSoDlv { get; set; }
    }
}
