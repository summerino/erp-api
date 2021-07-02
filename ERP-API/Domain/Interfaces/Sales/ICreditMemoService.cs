using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface ICreditMemoService : IGeneralService<CreditMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);
        List<dynamic> GetRelatedTransactions(string code);
        SaveResult Delete(string code, int userId);
    }
}
