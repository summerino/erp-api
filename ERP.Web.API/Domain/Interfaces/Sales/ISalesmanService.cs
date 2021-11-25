using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ISalesmanService : IGeneralService<SalesmanGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(string groupId, string startDate, string recurrence, string visitDay);

        IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(long id);

        IEnumerable<VwSalesmanSchedule> GetSalesmanScheduleWithDate(long id, string date);

        IEnumerable<VwSalesmanScheduleCustomer> GetSalesmanScheduleDetailData(List<long> id);

        #region Mobile
        SalesProfile GetSalesProfileForMobile(int id);
        SaveResult InsertTracking(SalesTracking data, int userId);
        #endregion
    }
}
