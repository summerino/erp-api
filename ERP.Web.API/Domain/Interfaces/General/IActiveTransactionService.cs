using ERP.Common;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General;

public interface IActiveTransactionService
{
    bool SeenByOthers(string src, string code, int userId);

    SaveResult LockedTransaction(ActiveTransactionRequest data, int userId);

    SaveResult ReleasedTransaction(ActiveTransactionRequest data, int userId);
}