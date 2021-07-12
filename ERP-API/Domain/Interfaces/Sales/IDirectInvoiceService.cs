using ERP.Entity.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Models.Sales;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface IDirectInvoiceService : IGeneralService<SalesInvoiceHeader>
    {
        DirectInvoiceRequest FindByCode(string code);

        SaveResult Insert(SalesInvoiceRequest data);

        SaveResult Update(SalesInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
