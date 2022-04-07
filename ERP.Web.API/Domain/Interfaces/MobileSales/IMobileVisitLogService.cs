using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileVisitLogService : IGeneralService<MobileVisitLog>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    SaveResult Approve(List<MobileVisitLog> data, int userId);

    SaveResult Reject(List<MobileVisitLog> data, int userId);

    dynamic GetRadius(string custCode, decimal? lat, decimal? lng);
}