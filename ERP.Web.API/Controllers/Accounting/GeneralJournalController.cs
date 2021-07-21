using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("general-journal")]
    [ApiController]
    public class GeneralJournalController : ControllerBase
    {
        private readonly IGeneralJournalService _gj;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IClosingMonthService _closingMonth;
        private const int _menuId = (int)Menu.GeneralJournal;

        public GeneralJournalController(IGeneralJournalService gj, ISystemParameterService sysPar,
            IClaimService claim, IAuthService auth, IClosingMonthService closingMonthService)
        {
            _gj = gj;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
            _closingMonth = closingMonthService;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _gj.GetData(
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
            var data = _gj.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.CoaCode,
                    x.Notes,
                    x.Type,
                    x.Amount
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(GeneralJournalRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Insert process
            data.Mark = "A";
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _gj.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, GeneralJournalRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _gj.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code, GeneralJournalRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _gj.Delete(data.Code, _claim.UserId);

            return Ok(result);
        }

        private (bool, string) Validate(GeneralJournalRequest data)
        {
            var periods = new List<string> { data.Date.ToString("yyyyMM") };
            if (data.OriginalDate.HasValue)
                periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.Date))
                return (false, "Tanggal tidak boleh lebih kecil dari tanggal mulai data.");

            if (data.Details != null)
            {
                if (!data.Details.Any())
                    return (false, "Detail tidak boleh kosong.");
            }

            return (true, "");
        }
    }
}
