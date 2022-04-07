using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using ERP.Web.API.Model.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileItemRequestService : IGeneralService<MobileItemRequestHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    IEnumerable<VwMobileItemRequestDetail> GetDetailData(string code);

    SaveResult Approve(List<MobileItemRequest> data, int userId, string date, string whCode, string notes);

    SaveResult Reject(List<MobileItemRequest> data, int userId);

    SaveResult Update(MobileItemRequest data);
}