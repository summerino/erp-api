using ERP.Entity.General;
using ERP.Entity.Sales;

namespace ERP.Web.API.Model.General
{
    public class EmployeeRequest : Employee
    {
        public IEnumerable<SalesmanSchedule> ScheduleDetails { get; set; }

        public IEnumerable<SalesmanScheduleCustomer> CustomerListDetails { get; set; }
    }
}
