using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.NetRevenue;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.NetRevenue;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class NetRevenueController : ControllerBase
{
    private readonly INetRevenueService _netRevenue;
    private readonly IClaimService _claim;

    public NetRevenueController(INetRevenueService netRevenue, IClaimService claim)
    {
        _netRevenue = netRevenue;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(int skip, int take, string filters, string sorts, string date)
    {
        var data =
            _netRevenue.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), _claim.UserId, date);
        var result = ((List<NetRevenueHeaderModel>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("income")]
    public IActionResult GetDetail(string date)
    {
        return Ok(_netRevenue.GetRevenueDetail(date, _claim.UserId));
    }

    [HttpGet("cost")]
    public IActionResult GetCostDetail(string date)
    {
        return Ok(_netRevenue.GetCostDetail(date, _claim.UserId));
    }

    [HttpGet("by-coa")]
    public IActionResult GetByCoa(int skip, int take, string filters, string sorts, string date, string coaCode)
    {
        return Ok(_netRevenue.GetRevenueDetailByCoa(
            skip, take,
            JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
            JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
            date, coaCode, _claim.UserId));
    }
}