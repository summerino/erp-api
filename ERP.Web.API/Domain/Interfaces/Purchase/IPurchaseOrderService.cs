using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Models.Mobile.Purchase;
using ERP.Web.API.Model.Purchase;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IPurchaseOrderService : IGeneralService<PurchaseOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwPurchaseOrderDetail> GetDetailData(string code, bool? fullReceived = null);

        List<dynamic> GetRelatedTransactions(string code);

        IEnumerable<VwPurchaseOrderHeader> GetInCompleteInvoiceData(string searchBy, string search, string invCode);

        SaveResult Insert(PurchaseOrderRequest data);

        SaveResult Update(PurchaseOrderRequest data);

        SaveResult Delete(string code, int userId);

        SaveResult Close(string code, int userId);

        #region Mobile
        DataSourceResult GetDataForMobile(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date);

        IEnumerable<PurchaseOrderDetailModel> GetDetailDataForMobile(string code, int srcTrans);

        DataSourceResult GetLogDataForMobile(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date);

        IEnumerable<ReceiveItemDetailModel> GetLogDetailDataForMobile(string code, int srcTrans);

        SaveResult InsertForMobile(PurchaseOrderRequestModel data, int UserId);
        #endregion
    }
}
