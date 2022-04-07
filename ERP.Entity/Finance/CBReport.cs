using System;

namespace ERP.Entity.Finance;

public class ReportByAllAccount
{
    public string Code { get; set; }

    public string Name { get; set; }

    public decimal BeginningBalance { get; set; }

    public decimal IncomingBalance { get; set; }

    public decimal OutgoingBalance { get; set; }

    public decimal EndingBalance { get; set; }
}

public class ReportByAccount
{
    public DateTime? Date { get; set; }

    public string Code { get; set; }

    public string Notes { get; set; }

    public decimal? IncomingBalance { get; set; }

    public decimal? OutgoingBalance { get; set; }

    public decimal EndingBalance { get; set; }

    public bool IsBold { get; set; }

}

public class ReportByAccountDetail
{
    public DateTime? Date { get; set; }

    public string Code { get; set; }

    public string Notes { get; set; }

    public string TransCode { get; set; }

    public string CoaCode { get; set; }

    public string CoaName { get; set; }

    public decimal? IncomingBalance { get; set; }

    public decimal? OutgoingBalance { get; set; }

    public decimal EndingBalance { get; set; }

    public bool IsBold { get; set; }
}