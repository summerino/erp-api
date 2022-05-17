using System;

namespace ERP.Entity.Sales;

public class ReportBySI
{
    public DateTime? Date { get; set; }

    public DateTime? DueDate { get; set; }

    public string Code { get; set; }

    public string OrderCode { get; set; }

    public string CustCode { get; set; }

    public string CustName { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Disc { get; set; }

    public decimal DiscHeader { get; set; }

    public decimal Dpp { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; }
}

public class ReportByDetailSI
{
    public DateTime? Date { get; set; }

    public DateTime? DueDate { get; set; }

    public string Code { get; set; }

    public string OrderCode { get; set; }

    public string TaxInvoiceNo { get; set; }

    public DateTime? TaxInvoiceDate { get; set; }

    public string CustCode { get; set; }

    public string CustName { get; set; }

    public string DoCode { get; set; }

    public string ItemInitial { get; set; }

    public string ItemName { get; set; }

    public int CategoryId { get; set; }

    public string CategoryInitial { get; set; }

    public decimal Qty { get; set; }

    public int UnitId { get; set; }

    public string UnitName { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? Disc { get; set; }

    public decimal? DiscHeader { get; set; }

    public decimal? Dpp { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? ExemptTaxAmount { get; set; }

    public decimal? NettPrice { get; set; }

    public decimal TotalGrossAmount { get; set; }

    public decimal TotalDisc { get; set; }

    public decimal TotalDiscHeader { get; set; }

    public decimal TotalAfterDisc { get; set; }

    public decimal TotalDpp { get; set; }

    public decimal TotalTaxAmount { get; set; }

    public decimal TotalExemptTaxAmount { get; set; }

    public decimal Total { get; set; }

    public decimal TotalNettPrice { get; set; }

    public string Status { get; set; }
}