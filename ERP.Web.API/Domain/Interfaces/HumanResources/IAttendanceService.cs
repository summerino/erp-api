using ERP.Common.Models;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.HumanResources
{
    public interface IAttendanceService
    {
        DataSourceResult GetDataReport(string startDate, string endDate, short employeeType, int sellerGroup, string employee, IEnumerable<Sort> sorts);

    }
}
