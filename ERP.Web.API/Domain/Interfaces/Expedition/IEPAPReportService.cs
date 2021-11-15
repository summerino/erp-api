using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Expedition
{
    public interface IEPAPReportService
    {
        DataSourceResult GetData(int type, string date, string supCode, IEnumerable<Sort> sorts);
    }
}
