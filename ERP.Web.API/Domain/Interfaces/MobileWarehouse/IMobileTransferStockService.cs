using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;

namespace ERP.Web.API.Domain.Interfaces.MobileWarehouse
{
    public interface IMobileTransferStockService : IGeneralService<MobileTransferStockHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwMobileTransferStockDetail> GetDetailData(string code);

        SaveResult Approve(List<MobileTransferStockHeader> data, int userId);

        SaveResult Reject(List<MobileTransferStockHeader> data, int userId);

        SaveResult Update(MobileTransferStockRequest data);
    }
}
