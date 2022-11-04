using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using ERP.Web.API.Model.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobilePaymentInvoiceService : IGeneralService<MobilePaymentInvoice>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    SaveResult Approve(MobilePaymentInvoiceApproveRequest data, int userId);

    SaveResult Reject(List<MobilePaymentInvoice> data, int userId);
}