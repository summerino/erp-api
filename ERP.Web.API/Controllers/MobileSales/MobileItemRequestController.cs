using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileSales;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileSales;

[Route("mobile-item-request")]
[ApiController]
public class MobileItemRequestController : ControllerBase
{
    private readonly IMobileItemRequestService _mir;
    private readonly IUnitOfMeasurementService _uom;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.MobileItemRequest;

    public MobileItemRequestController(IMobileItemRequestService mir, IUnitOfMeasurementService uom,
        IClaimService claim, IAuthService auth)
    {
        _mir = mir;
        _uom = uom;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _mir.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpGet("item")]
    public IActionResult GetDetailData(string code)
    {
        var uomC = _uom.GetDataConversion().ToList();

        var data = _mir.GetDetailData(code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.LineNo,
                x.ItemId,
                x.ItemName,
                x.UomId,
                x.UnitId,
                x.Qty,
                Units = uomC.Where(u => u.UomId == x.UomId)
                    .Select(u => new
                    {
                        u.Id,
                        u.UomId,
                        u.UnitToConvert,
                        u.UnitEquivalent,
                        u.Conversion,
                        u.IsBaseUnit,
                        u.Seq
                    })
                    .OrderBy(u => u.Seq)
                    .ToList(),
                x.UnitName
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPut("approve")]
    public IActionResult Approve(List<MobileItemRequest> data, string date, string whCode, string notes)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        var result = _mir.Approve(data, _claim.UserId, date, whCode, notes);

        return Ok(result);
    }

    [HttpPut("reject")]
    public IActionResult Reject(List<MobileItemRequest> data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        var result = _mir.Reject(data, _claim.UserId);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, MobileItemRequest data)
    {

        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _mir.Update(data);

        return Ok(result);
    }
}