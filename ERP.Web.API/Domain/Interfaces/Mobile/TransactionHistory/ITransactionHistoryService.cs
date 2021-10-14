using System.Collections.Generic;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory
{
    public interface ITransactionHistoryService
    {
        DataSourceResult GetDataByCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
        DataSourceResult GetDataByDate(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort);
        DataSourceResult GetDataByProduct(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
        DataSourceResult GetItemDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
        DataSourceResult GetCustomerDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);
        DataSourceResult GetDataCumulative(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int year,string custCode,int userId);
        DataSourceResult GetDataByLog(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string custCode,int userId);
    }
}
