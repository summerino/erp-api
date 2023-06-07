using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Entity.MobileSales;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;

namespace ERP.Web.API.Domain.Interfaces.Mobile.VisitOrder;

public interface IVisitOrderService
{
    DataSourceResult GetData(int userId, string lastUpdate);
    IEnumerable<MobileReason> GetReason(string lastUpdate);
    VwVisitOrder GetDetail(string code);
    IEnumerable<VwVisitOrderCustomer> GetVisitOrderCustomer(int userId, string lastUpdate);
    IEnumerable<MobileVisitLog> GetLogVisit(int userId, string lastUpdate);
    IEnumerable<VisitReasonModel> GetVisitReason(int userId, string lastUpdate);
    IEnumerable<VisitOrderInvoice> GetVisitOrderInvoice(int userId, string lastUpdate);
    IEnumerable<SalesInvoiceModel> GetSalesInvoceHeader(int userId, string lastUpdate);
    IEnumerable<PaymentInvoiceModel> GetMobilePaymentInvoice(int userId, string lastUpdate);
    IEnumerable<PromoHeader> GetPromoHeader(string lastUpdate);
    IEnumerable<PromoDetail> GetPromoDetail(string lastUpdate);
    IEnumerable<PromoDetailMultipleItem> GetPromoDetailMultipleItem(string lastUpdate);
    IEnumerable<PromoDetailTier> GetPromoDetailTier(string lastUpdate);
    IEnumerable<PromoSubject> GetPromoSubjects(string lastUpdate);
    IEnumerable<OrderHeaderModel> GetMobileOrderHeaders(int userId, string lastUpdate);
    IEnumerable<OrderDetailModel> GetMobileOrderDetails(int userId, string lastUpdate);
    IEnumerable<MobileOrderDetailDiscount> GetMobileOrderDetailDiscounts(int userId, string lastUpdate);
    IEnumerable<MobileOrderDetailFreeGood> GetMobileOrderDetailFreeGoods(int userId, string lastUpdate);
    IEnumerable<Tax> GetTax(string lastUpdate);
    IEnumerable<PaymentTerm> GetPaymentTerms(string lastUpdate);
    SaveResult Insert(VisitRequestModel data);
    SaveResult SubmitVisit(VisitSubmitModel data);
    IEnumerable<DateTime> GetListDate(int userId, DateTime date);
    IEnumerable<VisitLogByDateModel> GetVisitLogByDate(int userId, DateTime date);
    IEnumerable<MobilePaymentMethod> GetMobilePaymentMethod(string lastUpdate);
    VisitLogDetailModel GetDetailVisitLog(string code);
    OrderHeaderModel GetOrderHeader(string visitLogCode);
    IEnumerable<OrderDetailRequestModel> GetOrderDetailRequest(string orderCode);
    IEnumerable<ItemCategory> GetItemCategories(string lastUpdate);
}