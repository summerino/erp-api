using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface IVisitPlanService : IGeneralService<VisitPlanHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<VwVisitPlanDetail> GetDetailData(string code);

        IEnumerable<VwVisitPlanDetailCustomer> GetCustomerDetailData(List<long> id);

        SaveResult Insert(VisitPlanRequest data);

        SaveResult Update(VisitPlanRequest data);

        SaveResult Delete(string code, int userId);
    }
}
