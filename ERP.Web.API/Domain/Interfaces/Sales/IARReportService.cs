using ERP.Common.Models;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IARReportService
    {
        DataSourceResult GetData(int type, string date, string custCode, int slsId, IEnumerable<Sort> sorts);
    }
}
