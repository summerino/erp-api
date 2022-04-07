using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.MobileSales;

namespace ERP.Web.API.Domain.Services.MobileSales;

public class MobileActivityLogReportService : IMobileActivityLogReportService
{
    private readonly TenantContext _db;

    public MobileActivityLogReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(string username, string typeCode, string note, string startDate, string endDate)
    {
        var data = _db.MobileActivityLogs.ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            data = data.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate).AddDays(1).AddSeconds(-1)).ToList();
        }
        else if (!string.IsNullOrEmpty(startDate))
        {
            data = data.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            data = data.Where(x => x.Date <= Convert.ToDateTime(endDate).AddDays(1).AddSeconds(-1)).ToList();
        }

        if (!string.IsNullOrEmpty(username))
        {
            data = data.Where(x => x.Username == username).ToList();
        }

        if (!string.IsNullOrEmpty(typeCode))
        {
            data = data.Where(x => x.TypeCode == typeCode).ToList();
        }

        if (!string.IsNullOrEmpty(note))
        {
            data = data.Where(x => x.Notes.Contains(note)).ToList();
        }

        return data.AsQueryable().ToDataSourceResult(0, data.Count(), null, null);
    }
}