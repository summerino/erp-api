using ERP.Web.API.Domain.Models;
using ERP_API.Model.Accounting;

namespace ERP_API.Domain.Interfaces.Accounting
{
    public interface IJournalService
    {
        SaveResult PostingJournal(JournalRequest data);
    }
}
