using ERP.Common;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Interfaces.Mobile.General;

public interface IImageService
{
    SaveResult AddImage(ImageModel data);
}