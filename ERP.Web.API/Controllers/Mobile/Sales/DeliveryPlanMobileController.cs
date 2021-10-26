using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Models.Mobile.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Web.API.Domain.Interfaces.Mobile.Sales
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class DeliveryPlanController : ControllerBase
    {
        private readonly IDeliveryPlanMobileService _deliveryPlan;
        private readonly IClaimService _claim;
        public DeliveryPlanController(IDeliveryPlanMobileService deliveryPlan, IClaimService claim)
        {
            _deliveryPlan = deliveryPlan;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, string search, int skip, int take, string date)
        {
            var data =
                _deliveryPlan.GetData(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search, date);

            var result = ((List<DeliveryPlanHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            }); ;
        }

        [HttpGet("detail")]
        public IActionResult GetDetailData(string code, int srcTrans)
        {
            //var uomC = _uom.GetDataConversion().ToList();

            var data = _deliveryPlan.GetDetailData(code, srcTrans);

            return Ok(data);
        }

        [HttpGet("log")]
        public IActionResult GetLogData(string filters, string sorts, string search, int skip, int take, string date)
        {
            DataSourceResult data =
                _deliveryPlan.GetLogData(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search, date);

            List<dynamic> result = ((List<DeliverItemHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            }); ;
        }

        [HttpGet("logDetail")]
        public IActionResult GetLogDetailData(string code)
        {
            var data = _deliveryPlan.GetLogDetailData(code);

            return Ok(data);
        }


        [HttpPost]
        public IActionResult OnPost(DeliveryPlanRequestModel data)
        {
            //data.Mark = "A";
            //data.ReceiveBy = _claim.UserId;
            //data.CreatedBy = _claim.UserId;
            //data.CreatedDate = DateTime.Now;
            //data.UpdatedBy = data.CreatedBy;
            //data.UpdatedDate = data.CreatedDate;

            var result =
                _deliveryPlan.Insert(data, _claim.UserId);

            return Ok(result);
        }
    }
}
