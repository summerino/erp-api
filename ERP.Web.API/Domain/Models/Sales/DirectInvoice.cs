using ERP.Entity.Sales;

namespace ERP.Web.API.Domain.Models.Sales;

public class DirectInvoiceHeader : VwSalesInvoiceHeader
{
    public long SalesBy { get; set; }

    public string WarehouseCode { get; set; }

    public decimal ShipmentFee { get; set; }

    public decimal HandlingFee { get; set; }

    public decimal SubTotal { get; set; }

    public decimal FinalDiscPercent { get; set; }

    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Dpp { get; set; }
}