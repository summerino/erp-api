using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Domain.Models.Finance;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Finance;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Finance;

[Route("general-cash-bank")]
[ApiController]
public class CashBankController : ControllerBase
{
    private readonly ICashBankService _cb;
    private readonly IClosingMonthService _closingMonth;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private readonly IActiveTransactionService _activeTrans;
    private const int MenuId = (int)Menu.CashBank;

    public CashBankController(ICashBankService cb, IClosingMonthService closingMonth,
        ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
    {
        _cb = cb;
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
            _cb.GetData(
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
        var data = _cb.GetDetailData(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("ar")]
    public IActionResult GetDataAr(string cbCode, string search, string filters, string sorts, int skip, int take)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeAccountReceivable }).Any())
            return Ok(new ApiResponse{ TableData = new List<dynamic>() });

        var data =
            _cb.GetDataAR(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, cbCode);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpGet("ap")]
    public IActionResult GetDataAp(string cbCode, string search, string filters, string sorts, int skip, int take)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeAccountPayable }).Any())
            return Ok(new ApiResponse { TableData = new List<dynamic>() });

        var data =
            _cb.GetDataAP(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, cbCode);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpGet("ep-ap")]
    public IActionResult GetDataEpap(string cbCode, string search, string filters, string sorts, int skip, int take)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeExpeditionDebt }).Any())
            return Ok(new ApiResponse { TableData = new List<dynamic>() });

        var data =
            _cb.GetDataEPAP(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, cbCode);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpGet("credit-memo")]
    public IActionResult GetDataCreditMemo(string type, string cbCode, string search, string filters, string sorts, int skip, int take)
    {
        switch (type)
        {
            // Checking role authorization
            case "DPC" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeSalesDownPayment }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            case "RDPC" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeSalesDownPaymentReturn }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            case "SR" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypeSalesReturn }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            default:
            {
                var data =
                    _cb.GetDataCreditMemo(
                        skip, take,
                        JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                        JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                        search, cbCode, type);

                return Ok(new ApiResponse
                {
                    RowCount = data.Total,
                    TableData = data.Data.ToDynamicList()
                });
            }
        }
    }

    [HttpGet("debit-memo")]
    public IActionResult GetDataDebitMemo(string type, string cbCode, string search, string filters, string sorts, int skip, int take)
    {
        switch (type)
        {
            // Checking role authorization
            case "DPS" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypePurchaseDownPayment }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            case "RDPS" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypePurchaseDownPaymentReturn }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            case "PR" when !_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.CbTypePurchaseReturn }).Any():
                return Ok(new ApiResponse { TableData = new List<dynamic>() });
            default:
            {
                var data =
                    _cb.GetDataDebitMemo(
                        skip, take,
                        JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                        JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                        search, cbCode, type);

                return Ok(new ApiResponse
                {
                    RowCount = data.Total,
                    TableData = data.Data.ToDynamicList()
                });
            }
        }
    }

    [HttpPost]
    public IActionResult OnPost(CashBankRequest data)
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

        var result = _cb.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, CashBankRequest data)
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

        var result = _cb.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, CashBankRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _cb.Delete(data.Code, _claim.UserId, MenuId, _claim.RoleId);

        return Ok(result);
    }

    private (bool, string) Validate(CashBankRequest data, bool onDelete = false, bool checkSeenByOther = true)
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
            switch (data.Type)
            {
                case "D" when data.Amount < 0:
                    return (false, "Total nilai tidak boleh minus untuk tipe kas bank masuk.");
                case "C" when data.Amount > 0:
                    return (false, "Total nilai tidak boleh plus untuk tipe kas bank keluar.");
            }

            if (!data.ItemDetails.Any())
                return (false, "Detail tidak boleh kosong.");

            // Checking role authorization for item details
            var map = new MapCbTypeToAction();
            var actionIdLists =
                map.CbTypeToActions
                    .Where(t => data.ItemDetails.Select(i => i.Type).Distinct().Contains(t.Code))
                    .Select(t => t.ActionId).ToList();

            if (_auth.GetActions(MenuId, _claim.RoleId, actionIdLists).Count() != actionIdLists.Count)
                return (false, AppConstant.UnAuthMessage);
        }

        // Checking is data seen by others
        if (checkSeenByOther && !_activeTrans.SeenByOthers("CB", data.Code, _claim.UserId))
        {
            return (false, "data sedang digunakan oleh pengguna lain.");
        }

        return (true, "");
    }
}