using System;

namespace ERP.Entity.Purchase;

public class ReportByPR
{
    public DateTime? Date { get; set; }

    public string Code { get; set; }
        
    public string Type { get; set; }

    public string SupCode { get; set; }

    public string SupName { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal Dpp { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; }
}

public class ReportByDetailPR
{
    public DateTime? Date { get; set; }

    public string Code { get; set; }

    public string Type { get; set; }

    public string SupCode { get; set; }

    public string SupName { get; set; }

    public string ItemInitial { get; set; }

    public string ItemName { get; set; }

    public string WarehouseName { get; set; }

    public int CategoryId { get; set; }

    public string CategoryInitial { get; set; }

    public decimal Qty { get; set; }

    public int UnitId { get; set; }

    public string UnitName { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? Dpp { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? ExemptTaxAmount { get; set; }

    public decimal? NettPrice { get; set; }

    public decimal TotalGrossAmount { get; set; }

    public decimal TotalDpp { get; set; }

    public decimal TotalTaxAmount { get; set; }

    public decimal TotalExemptTaxAmount { get; set; }

    public decimal TotalNettPrice { get; set; }

    public string Status { get; set; }
}