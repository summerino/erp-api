using ERP.Common;
using ERP.Web.API.Domain.Models.General;

namespace ERP.Web.API.Domain.Interfaces.Mobile.General
{
    public interface IActivityLogService
    {
        SaveResult Insert(IEnumerable<LogActivityRequestModel> data, int userId);
    }
}
