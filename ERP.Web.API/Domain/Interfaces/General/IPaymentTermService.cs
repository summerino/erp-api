using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface IPaymentTermService : IGeneralService<PaymentTerm>
    {
        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(int id, int userId);
    }
}
