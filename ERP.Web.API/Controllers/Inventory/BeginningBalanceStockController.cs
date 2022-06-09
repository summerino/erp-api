using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Inventory;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Inventory;

[Route("bb-stock")]
[ApiController]

public class BeginningBalanceStockController : ControllerBase
{
    private readonly IBeginningBalanceStockService _bb;
    private readonly IUnitOfMeasurementService _uom;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;

    private const int _menuId = (int)Menu.BeginningBalanceStock;

    public BeginningBalanceStockController(IBeginningBalanceStockService bb, IUnitOfMeasurementService uom,
        IClosingMonthService closingMonth, ISystemParameterService sysPar, IClaimService claim, IAuthService auth)
    {
        _bb = bb;
        _uom = uom;
        _closingMonth = closingMonth;
        _sysPar = sysPar;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _bb.GetData(
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
        var data = _bb.GetDetailData(code)
            .Select(x => new
            {
                x.Id, x.Code, x.LineNo, x.ItemId, x.ItemName, x.UomId, x.UnitId, x.Qty,
                x.UnitPrice, x.Notes,
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
                OldUnitId = x.UnitId,
                OldUnitName = x.ItemUomBuyName,
                OldUnitPrice = x.ItemBuyPrice,
                Total = x.Qty * x.UnitPrice,
                State = ""
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }
        
    [HttpPost]
    public IActionResult OnPost(BeginningBalanceRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Insert process
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _bb.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, BeginningBalanceRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Update process
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _bb.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, BeginningBalanceRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Delete }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _bb.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    [HttpPost("upload")]
    public IActionResult OnUpload(IEnumerable<UploadBBStockDetailRequest> data)
    {

        var result = _bb.VerifyUpload(data);

        return Ok(new ApiResponse
        {
            RowCount = result.Count(),
            TableData = result.ToDynamicList()
        });
    }

    [HttpPost("posting")]
    public IActionResult OnPosting(UploadBBStockHeaderRequest data)
    {

        var result = _bb.Posting(data, _claim.UserId);

        return Ok(result);
    }

    private (bool, string) Validate(BeginningBalanceRequest data, bool onDelete = false)
    {
        var periods = new List<string> { data.Date.ToString("yyyyMM") };
        if (data.OriginalDate.HasValue)
            periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

        if (_closingMonth.IsMonthClosed(periods))
            return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

        // Checking data start date validity
        if (_sysPar.IsStartDateValid(data.Date))
            return (false, "Tanggal tidak boleh lebih besar dari tanggal mulai data.");

        if (!onDelete)
        {
            if (!data.ItemDetails.Any())
                return (false, "Detail tidak boleh kosong.");

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
                return (false, "Terdapat barang dengan satuan yang sama pada bagian detail.");
        }

        return (true, "");
    }
}