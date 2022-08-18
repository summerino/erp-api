using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models.Sales;

namespace ERP.Web.API.Model.Sales;

public class SalesInvoiceRequest : SalesInvoiceHeader
{
    public IEnumerable<SalesInvoiceDetail> Details { get; set; }
    public IEnumerable<SalesInvoiceCreditMemo> Memos { get; set; }
    public IEnumerable<SalesInvoiceCreditMemo> SalesDownPayments { get; set; }
    public IEnumerable<SalesOrderDetailRequest> ItemDetails { get; set; }
    public IEnumerable<SalesOrderPromo> ListPromo { get; set; }

    // Direct Invoice
    public int? PaymentTermId { get; set; }

    public long SalesBy { get; set; }

    public string WarehouseCode { get; set; }

    public decimal Rate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal FinalDiscPercent { get; set; }

    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Dpp { get; set; }
    // End - Direct Invoice

    public DateTime? OriginalDate { get; set; }

    public DateTime? OriginalDueDate { get; set; }

    public int CustTypeId { get; set; }
}

public class DirectInvoiceRequest : DirectInvoiceHeader
{
    public int? PaymentTermId { get; set; }
}