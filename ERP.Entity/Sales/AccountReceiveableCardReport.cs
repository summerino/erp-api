using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Sales
{
    public class ReportByARCard
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public string Notes { get; set; }

        public decimal? DebitAmount { get; set; }

        public decimal? CreditAmount { get; set; }

        public decimal? RemainingAmount { get; set; }

        public string CustCode { get; set; }

        public long? SalesId { get; set; }

        public bool IsBold { get; set; }
    }
}
