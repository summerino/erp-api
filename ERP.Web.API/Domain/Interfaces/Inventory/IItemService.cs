using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface IItemService : IGeneralService<Item>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string warehouseCode, string search, string mobileLastSync = null);

        SaveResult Delete(int id, int userId);

        IEnumerable<dynamic> GetRelatedOrderTrans(string whId, int itemId);

        IEnumerable<dynamic> GetRelatedIndentTrans(string whId, int itemId);

        IEnumerable<dynamic> GetRelatedTransferTrans(string whId, int itemId);

        bool IsItemUsed(int id);

        #region Mobile
        ItemInformationModel GetItemInformation(int itemId, string custCode);
        #endregion
    }
}
