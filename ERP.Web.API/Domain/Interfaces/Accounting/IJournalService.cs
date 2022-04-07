using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting;

public interface IJournalService
{
    void PostingJournal(JournalRequest data, int userId, int tenantId);

    IEnumerable<PostingLog> GetPostingHistory(JournalRequest data);

    PostingState GetPostingState(int userId);

    bool CheckPrevPeriod(DateTime postDate);
}