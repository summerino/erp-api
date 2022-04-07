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
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Sales;

[Route("visit-order")]
[ApiController]
public class VisitOrderController : ControllerBase
{
    private readonly IVisitOrderService _visitOrder;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private readonly IActiveTransactionService _activeTrans;
    private const int MenuId = (int)Menu.VisitOrder;

    public VisitOrderController(IVisitOrderService visitOrder, IClosingMonthService closingMonth,
        ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
    {
        _visitOrder = visitOrder;
        _closingMonth = closingMonth;
        _sysPar = sysPar;
        _claim = claim;
        _auth = auth;
        _activeTrans = activeTrans;
    }

    [HttpGet]
    public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
    {
        var data =
            _visitOrder.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                search);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpGet("visit-order-customer")]
    public IActionResult GetVisitOrderCustomer(string code)
    {
        var data =
            _visitOrder.GetVisitOrderCustomer(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("visit-order-invoice")]
    public IActionResult GetVisitOrderInvoice(string code)
    {
        var data =
            _visitOrder.GetVisitOrderInvoice(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(VisitOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, checkSeenByOther: false);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        data.Mark = "A";
        data.CreatedBy = 1;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _visitOrder.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, VisitOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        data.UpdatedBy = 1;
        data.UpdatedDate = DateTime.Now;

        var result = _visitOrder.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, VisitOrderRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _visitOrder.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    [HttpGet("verify-sales")]
    public IActionResult IsSalesHasScheduledVisitOrder(int salesId, string date)
    {
        var result = _visitOrder.IsSalesHasScheduledVisitOrder(salesId, date);

        return Ok(result);
    }

    private (bool, string) Validate(VisitOrderRequest data, bool checkSeenByOther = true)
    {
        var periods = new List<string> { data.Date.ToString("yyyyMM") };
        if (data.OriginalDate.HasValue)
            periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

        if (_closingMonth.IsMonthClosed(periods))
            return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

        // Checking is data seen by others
        if (checkSeenByOther && !_activeTrans.SeenByOthers("VO", data.Code, _claim.UserId))
        {
            return (false, "data sedang digunakan oleh pengguna lain.");
        }

        // Checking data start date validity
        return !_sysPar.IsStartDateValid(data.Date)
            ? (false, "Tanggal tidak boleh lebih kecil dari tanggal mulai data.")
            : (true, "");
    }
}