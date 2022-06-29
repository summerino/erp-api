using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesDownPaymentService : IGeneralService<CreditMemo>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search);

    List<dynamic> GetRelatedTransactions(string code);

    List<dynamic> GetReturnRelatedTransactions(string code);

    SaveResult Delete(string code, int userId);
}