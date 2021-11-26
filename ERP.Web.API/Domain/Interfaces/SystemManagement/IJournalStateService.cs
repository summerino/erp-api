using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface IJournalStateService
    {
        JournalState GetStatusPost(int userId);
    }
}
