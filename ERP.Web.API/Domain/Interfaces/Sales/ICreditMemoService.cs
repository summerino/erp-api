using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ICreditMemoService : IGeneralService<CreditMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        
        DataSourceResult GetDataOutstandingCreditMemo(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        
        List<dynamic> GetRelatedTransactions(string code);
        
        SaveResult Delete(string code, int userId);
    }
}
