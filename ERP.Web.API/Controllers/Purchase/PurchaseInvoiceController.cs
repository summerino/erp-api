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

namespace ERP.Web.API.Controllers.Purchase;

[Route("purchase-invoice")]
[ApiController]
public class PurchaseInvoiceController : ControllerBase
{
    private readonly IPurchaseInvoiceService _inv;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private readonly IActiveTransactionService _activeTrans;
    private const int MenuId = (int)Menu.PurchaseInvoice;

    public PurchaseInvoiceController(IPurchaseInvoiceService inv, IClosingMonthService closingMonth,
        ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
    {
        _inv = inv;
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
            _inv.GetData(
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

    [HttpGet("detail")]
    public IActionResult GetDetailData(string code)
    {
        var data = _inv.GetDetailData(code)
            .Select(x => new
            {
                x.Id, x.Code, x.LineNo, x.RcvCode, x.ShipmentFee, x.HandlingFee,
                x.SubTotal, x.FinalDisc, x.TaxAmount, x.ExemptTaxAmount, x.Total, x.Dpp,
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
        var data = _inv.GetRelatedTransactions(code)
            .Select(x => new
            {
                x.Code,
                x.Date,
                Total = x.Amount,
                Type = "Kas Bank"
            }).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("memo")]
    public IActionResult GetDataMemo(string code)
    {
        var data = _inv.GetDataMemo(code)
            .Select(x => new
            {
                x.Id,
                x.DebitMemoCode,
                x.Date,
                x.Type,
                x.DebitMemoAmount
            }).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(PurchaseInvoiceRequest data)
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

        var result = _inv.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, PurchaseInvoiceRequest data)
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

        var result = _inv.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, PurchaseInvoiceRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _inv.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    private (bool, string) Validate(PurchaseInvoiceRequest data, bool onDelete = false, bool checkSeenByOther = true)
    {
        var periods = new List<string> { data.Date.ToString("yyyyMM"), data.DueDate.ToString("yyyyMM") };
        if (data.OriginalDate.HasValue)
            periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));
        if (data.OriginalDueDate.HasValue)
            periods.Add(data.OriginalDueDate.Value.ToString("yyyyMM"));
            
        if (_closingMonth.IsMonthClosed(periods))
            return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

        // Checking data start date validity
        if (!_sysPar.IsStartDateValid(data.Date))
            return (false, "Tanggal Transaksi tidak boleh lebih kecil dari tanggal mulai data.");

        // Checking data start date validity
        if (!_sysPar.IsStartDateValid(data.DueDate))
            return (false, "Tanggal Jatuh Tempo tidak boleh lebih kecil dari tanggal mulai data.");

        if (!onDelete)
        {
                
            if (!data.Details.Any())
                return (false, "Detail tidak boleh kosong.");

            if (data.Details.GroupBy(x => new { x.RcvCode }).Any(x => x.Count() > 1))
                return (false, "Terdapat kode penerimaan yang sama pada bagian detail.");
            if (data.Memos.Any())
            {
                var paidAmount = data.Memos.Sum(x => x.DebitMemoAmount);
                if (paidAmount > data.Total)
                    return (false, "Nilai pembayaran lebih besar dari pada nilai transaksi.");
            }
        }

        // Checking is data seen by others
        if (checkSeenByOther && !_activeTrans.SeenByOthers("PI", data.Code, _claim.UserId))
        {
            return (false, "data sedang digunakan oleh pengguna lain.");
        }

        return (true, "");
    }
}