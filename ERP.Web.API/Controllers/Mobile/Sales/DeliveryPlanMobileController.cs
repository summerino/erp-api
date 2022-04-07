using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Models.Mobile.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ERP.Web.API.Domain.Interfaces.Mobile.Sales;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class DeliveryPlanMobileController : ControllerBase
{
    private readonly IDeliveryPlanMobileService _deliveryPlan;
    private readonly IClaimService _claim;
    public DeliveryPlanMobileController(IDeliveryPlanMobileService deliveryPlan, IClaimService claim)
    {
        _deliveryPlan = deliveryPlan;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(DateTime? date, string search)
    {
        var data = _deliveryPlan.GetData(date, search, _claim.UserId);

        return Ok(data);
    }

    [HttpGet("item")]
    public IActionResult GetDetailData(string code)
    {
        var data = _deliveryPlan.GetDetailData(code);

        return Ok(data);
    }

    [HttpGet("log")]
    public IActionResult GetLogData(string filters, string sorts, string search, int skip, int take, string date)
    {
        DataSourceResult data =
            _deliveryPlan.GetLogData(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, date, _claim.UserId);

        List<dynamic> result = ((List<DeliveryItemHeaderModel>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        }); ;
    }

    [HttpGet("logItem")]
    public IActionResult GetLogDetailData(string code)
    {
        var data = _deliveryPlan.GetLogDetailData(code);

        return Ok(data);
    }


    [HttpPost]
    public IActionResult OnPost(DeliveryPlanRequestModel data)
    {
        data.Mark = "A";
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result =
            _deliveryPlan.Insert(data, _claim.UserId);

        return Ok(result);
    }
}