using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Model.Accounting
{
    public class GeneralJournalRequest : GeneralJournalHeader
    {
        public IEnumerable<GeneralJournalDetailRequest> Details { get; set; }

        public decimal TotalDebit { get; set; }

        public decimal TotalCredit { get; set; }
    }

    public class GeneralJournalDetailRequest : GeneralJournalDetail
    {
        public decimal? DebitValue { get; set; }

        public decimal? CreditValue { get; set; }
    }
}
