using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.General
{
    public class EmployeeRequest : Employee
    {
        public IEnumerable<SalesmanSchedule> ScheduleDetails { get; set; }

        public IEnumerable<SalesmanScheduleCustomer> CustomerListDetails { get; set; }
    }
}
