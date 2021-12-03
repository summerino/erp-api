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
        public IActionResult GetDataPromotion(int skip, int take, string filters, string sorts, DateTime? date, string search)
        {
            var data = _customerPromotion.GetDataPromotion(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search, _claim.UserCode);

            var result = ((List<PromotionHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            });
        }

        [HttpGet("detail")]
        public IActionResult GetDataPromotion(string code)
        {
            var data = _customerPromotion.GetDataPromotionDetail(code);

            return Ok(data);
        }
    }
}
