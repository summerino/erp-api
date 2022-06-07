using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Purchase
{
    public class ReportByAPCard
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public string Notes { get; set; }

        public decimal? DebitAmount { get; set; }

        public decimal? CreditAmount { get; set; }

        public decimal? RemainingAmount { get; set; }

        public string SupCode { get; set; }

        public bool IsBold { get; set; }
    }
}
