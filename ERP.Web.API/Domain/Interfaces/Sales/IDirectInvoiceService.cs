using ERP.Common;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Domain.Models.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IDirectInvoiceService : IGeneralService<SalesInvoiceHeader>
    {
        DirectInvoiceRequest FindByCode(string code);

        SaveResult Insert(SalesInvoiceRequest data);

        SaveResult Update(SalesInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
