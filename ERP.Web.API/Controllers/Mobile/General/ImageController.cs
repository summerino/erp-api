using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Mobile.General
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _image;

        public ImageController(IImageService image)
        {
            _image = image;
        }

        [HttpPost]
        public IActionResult OnPost(ImageModel data)
        {
            var result = _image.AddImage(data);
            return Ok(result);
        }
    }
}
