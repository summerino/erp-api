using System;

namespace ERP.Entity.Sales;

public class ReportByCustomerMutation
{
    public string Code { get; set; }

    public string Name { get; set; }

    public int? TotalTrans { get; set; }

    public decimal BeginningBalance { get; set; }

    public decimal TransAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal EndingBalance { get; set; }
}

public class ReportByDeliveryARMutation
{
    public DateTime? Date { get; set; }

    public DateTime? DueDate { get; set; }

    public string Code { get; set; }

    public string SrcCode { get; set; }

    public string InvCode { get; set; }

    public string SlsName { get; set; }

    public string CustCode { get; set; }

    public string CustName { get; set; }

    public decimal BeginningBalance { get; set; }

    public decimal TransAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal EndingBalance { get; set; }
}