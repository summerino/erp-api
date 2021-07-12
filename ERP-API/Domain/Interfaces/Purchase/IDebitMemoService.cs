using System.Collections.Generic;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IDebitMemoService : IGeneralService<DebitMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        List<dynamic> GetRelatedTransactions(string code);
        SaveResult Delete(string code, int userId);

    }
}
