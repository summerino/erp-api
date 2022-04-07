using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.HumanResource;

public interface IAttendanceReportService
{
    DataSourceResult GetData(string startDate, string endDate, short employeeType, int salesGroupId,
        string employee, IEnumerable<Sort> sorts);
}