using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.HumanResource;

namespace ERP.Web.API.Domain.Services.HumanResource;

public class AttendanceReportService : IAttendanceReportService
{
    private readonly TenantContext _db;

    public AttendanceReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(string startDate, string endDate, short employeeType, int salesGroupId,
        string employee, IEnumerable<Sort> sorts)
    {
        var data = _db.VwAttendanceReports.AsQueryable();

        if (!string.IsNullOrEmpty(startDate))
            data = data.Where(x => x.Date >= Convert.ToDateTime(startDate));

        if (!string.IsNullOrEmpty(endDate))
            data = data.Where(x => x.Date <= Convert.ToDateTime(endDate));

        if(!string.IsNullOrWhiteSpace(employee))
            data = data.Where(x => x.Name.Contains(employee));

        if (employeeType > 0)
            data = data.Where(x => x.Type.Equals(employeeType));

        if (salesGroupId > 0)
            data = data.Where(x => x.SalesGroupId.Equals(salesGroupId));

        return data.ToDataSourceResult(0, data.Count(), null, sorts);
    }
}