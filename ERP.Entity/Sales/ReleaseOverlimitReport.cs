using System;

namespace ERP.Entity.Sales
{
    public class ReleaseOverlimitReport
    {
        public string Code { get; set; }

        public DateTime ReleasedDate { get; set; }

        public string ReleasedBy { get; set; }

        public string CustCode { get; set; }

        public string CustName { get; set; }

        public decimal Total { get; set; }
    }
}
