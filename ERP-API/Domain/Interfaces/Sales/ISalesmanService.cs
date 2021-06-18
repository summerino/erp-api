using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ISalesmanService : IGeneralService<SalesmanGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(string groupId, string startDate, string recurrence, string visitDay);

        IEnumerable<VwSalesmanScheduleCustomer> GetSalesmanScheduleDetailData(List<long> id);
    }
}
