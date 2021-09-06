namespace ERP.Entity.Accounting
{
    public class TrialBalanceResult
    {
        public string CoaCode { get; set; }

        public string CoaName { get; set; }

        public string CurrCode { get; set; }

        public string Sort { get; set; }

        public string IsBold { get; set; }

        public decimal? BeginBalIdr { get; set; }

        public decimal? DebetIdr { get; set; }

        public decimal? CreditIdr { get; set; }

        public decimal? EndBalIdr { get; set; }
    }
}
