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
    [Route("bb-ar")]
    [ApiController]
    public class BeginningBalanceAccountReceivableController : ControllerBase
    {
        private readonly IBeginningBalanceAccountReceivableService _bbAr;
        private readonly IClosingMonthService _closingMonth;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int MenuId = (int)Menu.BeginningBalanceAccountReceivable;

        public BeginningBalanceAccountReceivableController(IBeginningBalanceAccountReceivableService bbAr,
            IClosingMonthService closingMonth, ISystemParameterService sysPar,
            IClaimService claim, IAuthService auth)
        {
            _bbAr = bbAr;
            _closingMonth = closingMonth;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _bbAr.GetData(
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
        public IActionResult OnPost(BeginningBalanceARRequest data)
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

            var result = _bbAr.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, BeginningBalanceARRequest data)
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

            var result = _bbAr.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id, BeginningBalanceARRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _bbAr.Delete(data.Id, _claim.UserId);

            return Ok(result);
        }

        [HttpPost("upload")]
        public IActionResult OnUpload(IEnumerable<UploadBBARRequest> data)
        {
            var result = _bbAr.VerifyUpload(data);
            return Ok(new ApiResponse
            {
                RowCount = result.Count(),
                TableData = result.ToDynamicList()
            });
        }
        [HttpPost("posting")]
        public IActionResult OnPosting(IEnumerable<UploadBBARRequest> data)
        {
            var result = _bbAr.Posting(data, _claim.UserId);
            return Ok(result);
        }

        private (bool, string) Validate(BeginningBalanceARRequest data)
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
