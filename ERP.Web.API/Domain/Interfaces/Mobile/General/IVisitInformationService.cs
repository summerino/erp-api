using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Interfaces.Mobile.General
{
    public interface IVisitInformationService
    {
        VisitInformationModel GetVisitInformation(int year, int month, int userId);
    }
}
