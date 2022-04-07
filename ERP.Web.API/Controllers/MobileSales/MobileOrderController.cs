using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileSales;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileSales;

[Route("mobile-order")]
[ApiController]
public class MobileOrderController : ControllerBase
{
    private readonly IMobileOrderService _mo;
    private readonly IUnitOfMeasurementService _uom;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.MobileOrder;

    public MobileOrderController(IMobileOrderService mo, IUnitOfMeasurementService uom,
        IClaimService claim, IAuthService auth)
    {
        _mo = mo;
        _uom = uom;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _mo.GetData(
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

        var discData = _mo.GetDiscDetailData(code);

        var data = _mo.GetDetailData(code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.LineNo,
                x.ItemId,
                x.ItemInitial,
                x.ItemName,
                x.UomId,
                x.UnitId,
                x.UnitName,
                x.Qty,
                x.UnitPrice,
                x.Disc,
                x.TaxId,
                x.TaxAmount,
                x.NettPrice,
                x.Total,
                x.Dpp,
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
                OldUnitId = x.ItemUomSellId,
                OldUnitName = x.ItemUomSellName,
                OldUnitPrice = x.ItemSellPrice,
                TotTax = x.Qty * x.TaxAmount,
                TotDPP = x.Qty * x.Dpp,
                State = "",
                discPromo = discData.Where(d => d.OrderDetailId == x.Id)
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("free-item")]
    public IActionResult GetFreeDetailData(string code)
    {

        var data = _mo.GetFreeDetailData(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, MobileOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Update process
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _mo.Update(data);

        return Ok(result);
    }

    [HttpPut("approve")]
    public IActionResult Approve(List<MobileOrderHeader> data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        var result = _mo.Approve(data, _claim.UserId);

        return Ok(result);
    }

    [HttpPut("reject")]
    public IActionResult Reject(List<MobileOrderHeader> data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        var result = _mo.Reject(data, _claim.UserId);

        return Ok(result);
    }
}