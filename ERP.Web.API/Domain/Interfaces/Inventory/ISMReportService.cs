using ERP.Common.Models;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface ISMReportService
    {
        DataSourceResult GetData(int type, string startDate, string endDate, string whCode, int itemId, int typeUnit, bool isSM, IEnumerable<Sort> sorts);
    }
}
