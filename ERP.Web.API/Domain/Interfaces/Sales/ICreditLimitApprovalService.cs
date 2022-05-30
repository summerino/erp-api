using ERP.Common;
using ERP.Common.Models;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ICreditLimitApprovalService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search);

        SaveResult SaveChanges(List<SalesOrderRequest> data, int userId);
    }
}
