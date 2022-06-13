using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Purchase;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Interfaces.Inventory;

namespace ERP.Web.API.Controllers.Purchase;

[Route("purchase-receive")]
[ApiController]
public class PurchaseReceiveController : ControllerBase
{
    private readonly IPurchaseReceiveService _rcv;
    private readonly IUnitOfMeasurementService _uom;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private readonly IActiveTransactionService _activeTrans;
    private const int MenuId = (int)Menu.PurchaseReceive;

    public PurchaseReceiveController(IPurchaseReceiveService rcv, IClosingMonthService closingMonth, IUnitOfMeasurementService uom,
        ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
    {
        _rcv = rcv;
        _uom = uom;
        _closingMonth = closingMonth;
        _sysPar = sysPar;
        _claim = claim;
        _auth = auth;
        _activeTrans = activeTrans;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _rcv.GetData(
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

        var data = _rcv.GetDetailData(code)
            .Select(x => new
            {
                x.Id, x.Code, x.LineNo,
                PoDetailId = x.TransDetailId, x.ItemId, x.ItemInitial, x.ItemName,
                x.OrderQty, x.OutstandingQty, x.Qty,
                x.UomId, x.UnitId, x.UnitName,
                x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                x.UnitPrice, x.Disc, x.FinalDiscHeader, x.TaxId, x.TaxAmount, x.NettPrice, x.Total, x.Dpp, x.ExemptTaxAmount,
                x.WarehouseCode, x.Type,
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
                OldUnitId = x.ItemUomBuyId,
                OldUnitName = x.ItemUomBuyName,
                OldUnitPrice = x.ItemBuyPrice,
                TotTax = x.Qty * x.TaxAmount,
                TotExemptTax = x.Qty * x.ExemptTaxAmount,
                TotDPP = x.Qty * x.Dpp,
                TypeName = x.Type == 0 ? "Normal" : "Bonus",
                State = ""
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("related-trans")]
    public IActionResult GetRelatedTransactions(string code)
    {
        var data = _rcv.GetRelatedTransactions(code);

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("un-invoice")]
    public IActionResult GetUnInvoiceData(string poCode, string invCode)
    {
        var data = _rcv.GetUnInvoiceData(poCode, invCode).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(PurchaseReceiveRequest data)
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

        var result = _rcv.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, PurchaseReceiveRequest data)
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

        var result = _rcv.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, PurchaseReceiveRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _rcv.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    private (bool, string) Validate(PurchaseReceiveRequest data, bool onDelete = false, bool checkSeenByOther = true)
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

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId, x.Type }).Any(x => x.Count() > 1))
                return (false, "Terdapat barang dengan satuan yang sama pada bagian detail.");

            if (data.ItemDetails.Where(x => x.Type == 0).Sum(x => x.Qty) <= 0)
                return (false, "Jumlah qty barang yang diterima tidak boleh nol.");
        }

        // Checking is data seen by others
        if (checkSeenByOther && !_activeTrans.SeenByOthers("RCV", data.Code, _claim.UserId))
        {
            return (false, "data sedang digunakan oleh pengguna lain.");
        }

        return (true, "");
    }
}