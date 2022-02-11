using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Finance
{
    public class OutstandingChequeReport
    {
        public DateTime? Date { get; set; }

        public DateTime? ChequeDate { get; set; }

        public string Code { get; set; }

        public string CoaName { get; set; }

        public string CoaNameHeader { get; set; }

        public string ChequeNo { get; set; }

        public string TransCode { get; set; }

        public string ClientName { get; set; }

        public decimal BalanceIn { get; set; }

        public decimal BalanceOut { get; set; }
    }
}
