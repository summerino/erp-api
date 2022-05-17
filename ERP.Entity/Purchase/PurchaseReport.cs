namespace ERP.Entity.Purchase;

public class ReportByItemCategoryPurchase
{
    public int CategoryId { get; set; }

    public string Initial { get; set; }

    public string Name { get; set; }

    public int TotalTrans { get; set; }

    public decimal Qty { get; set; }

    public int UnitId { get; set; }

    public string UnitName { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Disc { get; set; }

    public decimal DiscHeader { get; set; }

    public decimal Dpp { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Total { get; set; }
}

public class ReportBySupplierPurchase
{
    public string Code { get; set; }

    public string Name { get; set; }

    public int TotalTrans { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Disc { get; set; }

    public decimal Dpp { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Total { get; set; }
}

public class ReportByItemPurchase
{
    public string Initial { get; set; }

    public string Name { get; set; }

    public int CategoryId { get; set; }

    public string CategoryInitial { get; set; }

    public int TotalTrans { get; set; }

    public decimal Qty { get; set; }

    public int UnitId { get; set; }

    public string UnitName { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Disc { get; set; }

    public decimal DiscHeader { get; set; }

    public decimal Dpp { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ExemptTaxAmount { get; set; }

    public decimal Total { get; set; }
}