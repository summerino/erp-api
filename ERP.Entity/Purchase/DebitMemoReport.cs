using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Purchase
{
    public class ReportByDebitMemo
    {
        public DateTime Date { get; set; }

        public string Code { get; set; }

        public string SrcCode { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal Amount { get; set; }

        public decimal UsedAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }
}
