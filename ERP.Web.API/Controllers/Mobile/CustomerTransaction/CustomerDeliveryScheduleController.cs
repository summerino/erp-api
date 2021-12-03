using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerDeliverySchedule;
using ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.CustomerDeliverySchedule
{
    [Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class CustomerDeliveryScheduleController : ControllerBase
    {
        private readonly ICustomerDeliveryScheduleService _customerDeliverySchedule;
        private readonly IClaimService _claim;

        public CustomerDeliveryScheduleController(ICustomerDeliveryScheduleService customerDeliverySchedule, IClaimService claim)
        {
            _customerDeliverySchedule = customerDeliverySchedule;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, int skip, int take, string date, string search)
        {
            var data =
                _customerDeliverySchedule.GetDataDeliveryScheduleHeader(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), date, search, _claim.UserCode);

            var result = ((List<DeliveryScheduleHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            }); ;
        }

        [HttpGet("detail")]
        public IActionResult GetDataDeliveryScheduleDetail(string code)
        {
            var data = _customerDeliverySchedule.GetDataDeliveryScheduleDetail(code);

            return Ok(data);
        }
    }
}
