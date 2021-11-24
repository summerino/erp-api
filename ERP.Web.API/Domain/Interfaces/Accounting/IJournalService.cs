using ERP.Common;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IJournalService
    {
        Task PostingJournal(JournalRequest data, int userId);

        IEnumerable<PostingLog> GetPostingHistory(JournalRequest data);

        bool CheckPrevPeriod(DateTime postDate);
    }
}
