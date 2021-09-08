using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Purchase;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IDebitMemoService : IGeneralService<DebitMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        
        DataSourceResult GetDataOutstandingDebitMemo(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        
        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Delete(string code, int userId);
    }
}
