using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.ItemRequest;
using ERP.Web.API.Domain.Models.Mobile.ItemRequest;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.ItemRequest;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class ItemRequestController : ControllerBase
{
    private readonly IItemRequestService _itemRequest;
    private readonly IClaimService _claim;

    public ItemRequestController(IItemRequestService itemRequest, IClaimService claim)
    {
        _itemRequest = itemRequest;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string filters, string sorts, int skip, int take, int? areaId)
    {
        var data =
            _itemRequest.GetData(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), _claim.UserId,areaId);

        var result = data.Data.ToDynamicList();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("detail")]
    public IActionResult GetDetail(string code)
    {
        var data = _itemRequest.GetDetail(code);

        var result = data.ToDynamicList();

        return Ok(result);
    }

    [HttpPost]
    public IActionResult OnPost(ItemRequestModel data)
    {
        // Insert process
        data.Mark = "A";
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _itemRequest.Insert(data);

        return Ok(result);
    }
}