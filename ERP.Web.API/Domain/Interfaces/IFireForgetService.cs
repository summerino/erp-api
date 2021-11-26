using ERP.Common;
using ERP.Web.API.Domain.Interfaces.Accounting;

namespace ERP.Web.API.Domain.Interfaces
{
    public interface IFireForgetService
    {
        void Execute(Func<IJournalService, SaveResult> DoWork);
    }
}
