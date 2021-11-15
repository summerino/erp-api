using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobilePaymentInvoiceService : IGeneralService<MobilePaymentInvoice>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Approve(List<MobilePaymentInvoice> data, int userId);

        SaveResult Reject(List<MobilePaymentInvoice> data, int userId);
    }
}
