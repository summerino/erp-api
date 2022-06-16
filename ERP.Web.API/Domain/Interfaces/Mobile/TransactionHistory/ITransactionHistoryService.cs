using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

namespace ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;

public interface ITransactionHistoryService
{
    DataSourceResult GetDataByCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search);
    DataSourceResult GetDataByDate(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate);
    DataSourceResult GetDataByProduct(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search);
    IEnumerable<TransactionHistoryByUnitProduct> GetDataByUnitProduct(int filterUnit, DateTime? startDate, DateTime? endDate, int userId);
    DataSourceResult GetItemDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
    DataSourceResult GetCustomerDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
    DataSourceResult GetDataCumulative(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int year, string custCode, int userId);
    DataSourceResult GetDataByLog(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string custCode, int userId);
    IEnumerable<ItemSubGroupModel> GetSubGroup();
    IEnumerable<ItemGroup> GetItemGroup();
    IEnumerable<ItemGroupSubGroup> GetItemSubGroup(int groupId);
    DataSourceResult GetDataBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, int groupId, string subGroup, int userId);
    DataSourceResult GetItemBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime date, int groupId, string subGroup);
    DataSourceResult GetDataBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int? groupId, int? subGroupId, int userId);
    DataSourceResult GetDataDetailBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int filterGroupId, int groupId, int subGroupId, int userId);
    DataSourceResult GetDataItemBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, string detailSubGroup, int userId);
}