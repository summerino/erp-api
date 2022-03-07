using ERP.Common;
using ERP.Common.Models;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General;

public interface IActiveTransactionService
{
    bool SeenByOthers(string src, string code, int userId);

    SaveResult LockedTransaction(ActiveTransactionRequest data, int userId);

    SaveResult ReleasedTransaction(ActiveTransactionRequest data, int userId);

    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

    SaveResult ReleaseTransaction(List<ActiveTransactionRequest> data);
}