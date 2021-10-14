using ERP.Common;

namespace ERP.Web.API.Domain.Interfaces.Mobile.General
{
    public interface IImageService
    {
        SaveResult AddImage(string base64image, string imageName);
    }
}
