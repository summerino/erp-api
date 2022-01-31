using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.CustomerTransaction
{
    [Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]

    public class CustomerPromotionController : ControllerBase
    {
        private readonly ICustomerPromotionService _customerPromotion;
        private readonly IClaimService _claim;

        public CustomerPromotionController(ICustomerPromotionService customerPromotion, IClaimService claim)
        {
            _customerPromotion = customerPromotion;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search)
        {
            var result = _customerPromotion.GetDataPromotion(search, _claim.UserCode);

            return Ok(result);
        }

        [HttpGet("detail")]
        public IActionResult GetDataPromotion(string code)
        {
            var data = _customerPromotion.GetDataPromotionDetail(code);

            return Ok(data);
        }
    }
}
