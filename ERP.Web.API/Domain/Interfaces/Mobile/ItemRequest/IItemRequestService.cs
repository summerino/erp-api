using ERP.Common;
using ERP.Common.Models;
using ERP.Web.API.Domain.Models.Mobile.ItemRequest;

namespace ERP.Web.API.Domain.Interfaces.Mobile.ItemRequest
{
    public interface IItemRequestService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, int userId, int? areaId);

        IEnumerable<ItemRequestDetail> GetDetail(string code);

        SaveResult Insert(ItemRequestModel data);
    }
}
