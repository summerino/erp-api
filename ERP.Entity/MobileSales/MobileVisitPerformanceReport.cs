using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.MobileSales
{
    public class MobileVisitPerformanceReport
    {
        public DateTime Date { get; set; }

        public int SalesmanId { get; set; }

        public int Scheduled { get; set; }

        public int Visited { get; set; }

        public int ScheduledInvoiced { get; set; }

        public int Unscheduled { get; set; }

        public int UnscheduledInvoiced { get; set; }
    }
}
