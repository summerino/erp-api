using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IDebitMemoService : IGeneralService<DebitMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        List<dynamic> GetRelatedTransactions(string code);
    }
}
