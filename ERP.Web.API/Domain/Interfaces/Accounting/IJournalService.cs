using ERP.Common;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IJournalService
    {
        SaveResult PostingJournal(JournalRequest data);

    }
}
