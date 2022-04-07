using System;

namespace ERP.Entity.Purchase;

public class ReportBySupplierAging
{
    public string Code { get; set; }

    public string Name { get; set; }

    public int? TotalTrans { get; set; }

    public decimal RemainderAmount { get; set; }

    public decimal Past90 { get; set; }

    public decimal Past61To90 { get; set; }

    public decimal Past31To60 { get; set; }

    public decimal Past15To30 { get; set; }

    public decimal Past8To14 { get; set; }

    public decimal Past1To7 { get; set; }

    public decimal DueToday { get; set; }

    public decimal Due1To7 { get; set; }

    public decimal Due8To14 { get; set; }

    public decimal Due15To30 { get; set; }

    public decimal Due31To60 { get; set; }

    public decimal Due61To90 { get; set; }

    public decimal Due90 { get; set; }
}

public class ReportByInvoiceAPAging
{
    public DateTime? Date { get; set; }

    public DateTime? DueDate { get; set; }

    public string Code { get; set; }

    public string OrderCode { get; set; }

    public string SupCode { get; set; }

    public string SupName { get; set; }

    public decimal RemainderAmount { get; set; }

    public decimal Past90 { get; set; }

    public decimal Past61To90 { get; set; }

    public decimal Past31To60 { get; set; }

    public decimal Past15To30 { get; set; }

    public decimal Past8To14 { get; set; }

    public decimal Past1To7 { get; set; }

    public decimal DueToday { get; set; }

    public decimal Due1To7 { get; set; }

    public decimal Due8To14 { get; set; }

    public decimal Due15To30 { get; set; }

    public decimal Due31To60 { get; set; }

    public decimal Due61To90 { get; set; }

    public decimal Due90 { get; set; }
}