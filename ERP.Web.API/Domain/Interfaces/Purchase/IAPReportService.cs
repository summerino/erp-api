using ERP.Common.Models;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IAPReportService
    {
        DataSourceResult GetData(int type, string date, string supCode, IEnumerable<Sort> sorts);
    }
}
