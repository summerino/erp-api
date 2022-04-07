using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileReasonService : IGeneralService<MobileReason>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    SaveResult Delete(int id, int userId);
}