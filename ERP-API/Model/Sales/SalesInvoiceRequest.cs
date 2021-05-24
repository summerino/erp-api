using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesInvoiceRequest : SalesInvoiceHeader
    {
        public IEnumerable<SalesInvoiceDetail> Details { get; set; }

        public IEnumerable<SalesOrderDetail> ItemDetails { get; set; }

        // Direct Invoice
        public long SalesBy { get; set; }

        public string WarehouseCode { get; set; }

        public decimal Rate { get; set; }

        public decimal SubTotal { get; set; }

        public decimal FinalDiscPercent { get; set; }

        public decimal FinalDisc { get; set; }

        public bool IncludeTax { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Dpp { get; set; }
        // End - Direct Invoice
    }
}
