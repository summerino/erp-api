using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface IVisitOrderService : IGeneralService<VisitOrder>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        SaveResult Insert(VisitOrderRequest data);

        SaveResult Update(VisitOrderRequest data);

        SaveResult Delete(string code, int userId);
    }
}
