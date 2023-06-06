using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-order")]
[ApiController]
public class SalesOrderController : ControllerBase
{
    private readonly ISalesOrderService _so;
    private readonly IUnitOfMeasurementService _uom;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private readonly IActiveTransactionService _activeTrans;
    private const int MenuId = (int)Menu.SalesOrder;

    public SalesOrderController(ISalesOrderService so, IUnitOfMeasurementService uom,
        IClosingMonthService closingMonth, ISystemParameterService sysPar,
        IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
    {
        _so = so;
        _uom = uom;
        _closingMonth = closingMonth;
        _sysPar = sysPar;
        _auth = auth;
        _claim = claim;
        _activeTrans = activeTrans;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data = 
            _so.GetData(
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
    public IActionResult GetDetailData(string code, bool? fullReceived)
    {
        var uomC =_uom.GetDataConversion().ToList();

        var discData = _so.GetDiscDetailData(code);

        var data = _so.GetDetailData(code, fullReceived)
            .Select(x => new
            {
                x.Id, x.Code, x.LineNo, x.ItemId, x.ItemInitial, x.ItemName,
                x.UomId, x.UnitId, x.UnitName, x.Qty,
                x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                x.QtyDlv, x.UnitPrice, x.Disc, x.FinalDiscHeader, x.TaxId, x.TaxAmount, x.ExemptTaxAmount,
                x.NettPrice, x.Total, x.Dpp, x.Notes,
                x.CoaInventory, x.CoaCogs, x.CoaSls, x.CoaSlsDisc, x.CoaSlsReturn,
                Units = uomC.Where(u => u.UomId == x.UomId)
                    .Select(u => new
                    {
                        u.Id, u.UomId, u.UnitToConvert, u.UnitEquivalent,
                        u.Conversion, u.IsBaseUnit, u.Seq
                    })
                    .OrderBy(u => u.Seq)
                    .ToList(),
                OldUnitId = x.ItemUomSellId,
                OldUnitName = x.ItemUomSellName,
                OldUnitPrice = x.ItemSellPrice,
                TotTax = x.Qty * x.TaxAmount,
                TotDPP = x.Qty * x.Dpp,
                TotFDH = x.Qty * x.FinalDiscHeader,
                TotUnitPrice = x.Qty * x.UnitPrice,
                TotDisc = x.Qty * x.Disc,
                State = "",
                discPromo = discData.Where(d => d.OrderDetailId == x.Id).OrderBy(d => d.LineNo)
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("free-item")]
    public IActionResult GetFreeDetailData(string code, bool? fullDlv)
    {

        var data = _so.GetFreeDetailData(code, fullDlv).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }


    [HttpGet("related-trans")]
    public IActionResult GetRelatedTransactions(string code)
    {
        var data = _so.GetRelatedTransactions(code);
            
        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("in-complete-invoice")]
    public IActionResult GetInCompleteInvoiceData(string searchBy, string search, string invCode)
    {
        var data = _so.GetInCompleteInvoiceData(searchBy, search, invCode).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("promos")]
    public IActionResult GetPromos(string code)
    {
        var data = _so.GetSalesOrderPromos(code);

        return Ok(new ApiResponse
        {
            RowCount = data.Count(),
            TableData = data.ToDynamicList()
        });
    }

    [HttpPost]
    public IActionResult OnPost(SalesOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, checkSeenByOther: false);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Insert process
        data.Mark = "A";
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;
            
        var result = _so.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, SalesOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Update process
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _so.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, SalesOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _so.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    [HttpPut("close/{code}")]

    public IActionResult OnClose(string code, SalesOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Close }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _so.Close(code, _claim.UserId);

        return Ok(result);
    }

    [HttpPut("check-over-limit")]
    public IActionResult CheckOverLimit(SalesOrderRequest data)
    {
        //Validate Process
        var result = _so.CheckOverLimit(data);

        return Ok(result);
    }

    [HttpPost("multi-save")]
    public IActionResult MultipleSave(MultipleSalesOrderRequest data)
    {
        var result = _so.MultipleSave(data);

        return Ok(result);
    }

    private (bool, string) Validate(SalesOrderRequest data, bool onDelete = false, bool checkSeenByOther = true)
    {
        var periods = new List<string> { data.Date.ToString("yyyyMM") };
        if (data.OriginalDate.HasValue)
            periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

        if (_closingMonth.IsMonthClosed(periods))
            return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

        // Checking data start date validity
        if (!_sysPar.IsStartDateValid(data.Date))
            return (false, "Tanggal tidak boleh lebih kecil dari tanggal mulai data.");

        if (!onDelete)
        { 
            if (!data.ItemDetails.Any())
                return (false, "Detail tidak boleh kosong.");

            //if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
            //    return (false, "Terdapat barang dengan satuan yang sama pada bagian detail.");

            //if (data.ItemDetails.Any(x => x.NettPrice <= 0))
            //    return (false, "Terdapat barang dengan nilai minus.");

            //if (data.Total <= 0)
            //    return (false, "Nilai total tidak boleh minus.");
        }

        // Checking is data seen by others
        if (checkSeenByOther && !_activeTrans.SeenByOthers("SO", data.Code, _claim.UserId))
        {
            return (false, "data sedang digunakan oleh pengguna lain.");
        }

        return (true, "");
    }
}