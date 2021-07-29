using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IVisitOrderService : IGeneralService<VisitOrder>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<VwVisitOrderCustomer> GetVisitOrderCustomer(string code);

        IEnumerable<VwVisitOrderInvoice> GetVisitOrderInvoice(string code);

        SaveResult Insert(VisitOrderRequest data);

        SaveResult Update(VisitOrderRequest data);

        SaveResult Delete(string code, int userId);
    }
}
