using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Finance;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("inter-cash-bank")]
    [ApiController]
    public class InterCashBankController : ControllerBase
    {
        private readonly IInterCashBankService _interCb;
        private readonly IClosingMonthService _closingMonth;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IActiveTransactionService _activeTrans;
        private const int MenuId = (int)Menu.InterCashBank;

        public InterCashBankController(IInterCashBankService interCb, IClosingMonthService closingMonth,
            ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IActiveTransactionService activeTrans)
        {
            _interCb = interCb;
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
                _interCb.GetData(
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
            var data = _interCb.GetDetailData(code).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(CashBankRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Checking coa code from & to can't be same
            if (data.ItemDetails.GroupBy(x => x.CoaCode).Any(g => g.Count() > 1))
                return Ok(new SaveResult(false, "Akun asal dan akun tujuan tidak boleh sama."));

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

            var result = _interCb.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, CashBankRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Checking coa code from & to can't be same
            if (data.ItemDetails.GroupBy(x => x.CoaCode).Any(g => g.Count() > 1))
                return Ok(new SaveResult(false, "Akun asal dan akun tujuan tidak boleh sama."));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _interCb.Update(data);

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

            var result = _interCb.Delete(data.Code, data.TransCode, _claim.UserId);

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

            if (!onDelete && (!data.ItemDetails.Any() || data.ItemDetails.Count != 2))
                return (false, "Detail Akun asal atau akun tujuan tidak ada.");

            // Checking is data seen by others
            if (checkSeenByOther && !_activeTrans.SeenByOthers("ICB", data.Code, _claim.UserId))
            {
                return (false, "data sedang digunakan oleh pengguna lain.");
            }

            return (true, "");
        }
    }
}
