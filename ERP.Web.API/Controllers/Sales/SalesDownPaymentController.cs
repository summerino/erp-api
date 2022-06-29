using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-down-payment")]
[ApiController]
public class SalesDownPaymentController : ControllerBase
{
    private readonly ISalesDownPaymentService _sdp;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;

    private const int MenuId = (int)Menu.SalesDownPayment;

    public SalesDownPaymentController(ISalesDownPaymentService sdp, IClosingMonthService closingMonth,
        ISystemParameterService sysPar, IClaimService claim, IAuthService auth)
    {
        _sdp = sdp;
        _closingMonth = closingMonth;
        _sysPar = sysPar;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _sdp.GetData(
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

    [HttpGet("related-trans")]
    public IActionResult GetRelatedTransactions(string code)
    {
        var data = _sdp.GetRelatedTransactions(code);

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("return-related-trans")]
    public IActionResult GetReturnRelatedTransactions(string code)
    {
        var data = _sdp.GetReturnRelatedTransactions(code);

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(CreditMemoRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        data.Mark = "PP"; // Pending Payment
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _sdp.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, CreditMemoRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _sdp.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, CreditMemoRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _sdp.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    private (bool, string) Validate(CreditMemoRequest data)
    {
        var periods = new List<string> { data.Date.ToString("yyyyMM") };
        if (data.OriginalDate.HasValue)
            periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

        if (_closingMonth.IsMonthClosed(periods))
            return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

        if (data.SrcTrans == 2)
            return (false, "data tidak dapat disimpan karena sumber transaksi adalah retur.");

        // Checking data start date validity
        return !_sysPar.IsStartDateValid(data.Date)
            ? (false, "Tanggal tidak boleh lebih kecil dari tanggal mulai data.")
            : (true, "");
    }
}