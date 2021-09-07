using System;

namespace ERP.Entity.Accounting
{
    public class ReportJournalResult
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string CoaCode { get; set; }

        public string CoaName { get; set; }

        public string Notes { get; set; }

        public string RefCode1 { get; set; }

        public string RefCode2 { get; set; }

        public string RefCode3 { get; set; }

        public string RefCode4 { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal DebetOc { get; set; }

        public decimal CreditOc { get; set; }
    }

    public class JournalReportModel
    {
        public string RptBy { get; set; }

        public string VouFrom { get; set; }

        public string RptDet { get; set; }

        public string CoaCode { get; set; }

        public string Src { get; set; }

        public string Acc { get; set; }

        public string Acc2 { get; set; }

        public string Curr { get; set; }

        public string Sort { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }

    public class JournalReportWrapper
    {
        public string AccCode { get; set; }

        public string AccName { get; set; }

        public string Notes { get; set; }

        public string RefCode1 { get; set; }

        public string RefCode2 { get; set; }

        public string RefCode3 { get; set; }

        public string RefCode4 { get; set; }

        public string CurrCode { get; set; }

        public string Rate { get; set; }

        public string DebetOc { get; set; }

        public string CreditOc { get; set; }

        public string EndBalOc { get; set; }

        public byte IsBold { get; set; }

        public DateTime? Date { get; set; }
    }
}
