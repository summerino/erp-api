using System.Collections.Generic;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ISalesmanService : IGeneralService<SalesmanGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(string groupId, string startDate, string recurrence, string visitDay);

        IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(long id);

        IEnumerable<VwSalesmanScheduleCustomer> GetSalesmanScheduleDetailData(List<long> id);
    }
}
