using System;

namespace ERP.Entity.MobileSales;

public class MobileVisitPerformanceReport
{
    public DateTime Date { get; set; }

    public long SalesmanId { get; set; }

    public string SalesmanName { get; set; }

    public int Scheduled { get; set; }

    public int Visited { get; set; }

    public int ScheduledInvoiced { get; set; }

    public int Unscheduled { get; set; }

    public int UnscheduledInvoiced { get; set; }
}