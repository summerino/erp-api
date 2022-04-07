using System;

namespace ERP.Entity.Sales;

public class ReportByST
{
    public long? SalesId { get; set; }

    public string SalesName { get; set; }

    public int? ItemGroupId { get; set; }

    public string ItemGroup { get; set; }

    public int? ItemSubGroupId { get; set; }

    public string ItemSubGroup { get; set; }

    public string ItemSubGroup2 { get; set; }

    public int? TotalCustomers { get; set; }

    public decimal? TargetAmount { get; set; }

    public decimal? RealAmount { get; set; }

    public decimal? TargetPercent { get; set; }
}

public class ReportByDetailST
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

    public int? CategoryId { get; set; }

    public string CategoryInitial { get; set; }

    public decimal? Qty { get; set; }

    public int? UnitId { get; set; }

    public string UnitName { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? Disc { get; set; }

    public decimal? DiscHeader { get; set; }

    public decimal? Dpp { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? NettPrice { get; set; }

    public decimal? TotalGrossAmount { get; set; }

    public decimal? TotalDisc { get; set; }

    public decimal? TotalDiscHeader { get; set; }

    public decimal? TotalAfterDisc { get; set; }

    public decimal? TotalDpp { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? Total { get; set; }

    public decimal? TotalNettPrice { get; set; }

    public string Status { get; set; }

    public string SubGroup1 { get; set; }

    public string SubGroup2 { get; set; }

    public string SubGroup3 { get; set; }

    public string SubGroup4 { get; set; }

    public string SubGroup5 { get; set; }

    public long? SalesId { get; set; }

    public int? ItemGroupId { get; set; }

    public int? ItemSubGroupId { get; set; }

}

public class ReportByTarget
{
    public long? SalesId { get; set; }

    public int? ItemGroupId { get; set; }

    public int? ItemSubGroupId { get; set; }

    public string ItemSubGroup2 { get; set; }

    public decimal? TargetAmount { get; set; }
}