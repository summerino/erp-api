using System;

namespace ERP.Entity.Accounting
{
    public class GeneralLedgerResult
    {
        public string CoaCode { get; set; }

        public string CoaName { get; set; }

        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public string Notes { get; set; }

        public string RefCode1 { get; set; }

        public string RefCode2 { get; set; }

        public string RefCode3 { get; set; }

        public string RefCode4 { get; set; }

        public string CurrCode { get; set; }

        public decimal? Rate { get; set; }

        public string Sort { get; set; }

        public string IsBold { get; set; }

        public decimal? DebetOc { get; set; }

        public decimal? CreditOc { get; set; }

        public decimal? EndBalOc { get; set; }
    }
}
