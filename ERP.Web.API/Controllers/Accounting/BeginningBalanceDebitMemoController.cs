using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Accounting;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("bb-debit-memo")]
    [ApiController]
    public class BeginningBalanceDebitMemoController : ControllerBase
    {
        private readonly IBeginningBalanceDebitMemoService _bbDm;
        private readonly IClosingMonthService _closingMonth;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int MenuId = (int)Menu.BeginningBalanceDebitMemo;

        public BeginningBalanceDebitMemoController(IBeginningBalanceDebitMemoService bbDm,
            IClosingMonthService closingMonth, ISystemParameterService sysPar,
            IClaimService claim, IAuthService auth)
        {
            _bbDm = bbDm;
            _closingMonth = closingMonth;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _bbDm.GetData(
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

        [HttpPost]
        public IActionResult OnPost(BeginningBalanceDebitMemoRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _bbDm.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, BeginningBalanceDebitMemoRequest data)
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

            var result = _bbDm.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id, BeginningBalanceDebitMemoRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _bbDm.Delete(data.Id, _claim.UserId);

            return Ok(result);
        }

        [HttpPost("upload")]
        public IActionResult OnUpload(IEnumerable<UploadBBDMRequest> data)
        {
            var result = _bbDm.VerifyUpload(data);
            return Ok(new ApiResponse
            {
                RowCount = result.Count(),
                TableData = result.ToDynamicList()
            });
        }
        [HttpPost("posting")]
        public IActionResult OnPosting(IEnumerable<UploadBBDMRequest> data)
        {
            var result = _bbDm.Posting(data, _claim.UserId);
            return Ok(result);
        }

        private (bool, string) Validate(BeginningBalanceDebitMemoRequest data)
        {
            var periods = new List<string> { data.Date.ToString("yyyyMM") };
            if (data.OriginalDate.HasValue)
                periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            return _sysPar.IsStartDateValid(data.Date)
                ? (false, "Tanggal tidak boleh lebih besar dari tanggal mulai data.")
                : (true, "");
        }
    }
}
