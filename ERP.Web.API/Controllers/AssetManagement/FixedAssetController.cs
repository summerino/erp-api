using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.AssetManagement;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.AssetManagement;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.AssetManagement
{
    [Route("fixed-asset")]
    [ApiController]
    public class FixedAssetController : ControllerBase
    {
        private readonly IFixedAssetService _fixedAsset;
        private readonly IClosingMonthService _closingMonth;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int _menuId = (int)Menu.FixedAsset;

        public FixedAssetController(IFixedAssetService fixedAsset, IClosingMonthService closingMonth,
            ISystemParameterService sysPar, IClaimService claim, IAuthService auth)
        {
            _fixedAsset = fixedAsset;
            _closingMonth = closingMonth;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _fixedAsset.GetData(
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
        public IActionResult OnPost(FixedAssetRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Insert }).Any())
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

            var result = _fixedAsset.Insert(data);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string code, FixedAssetRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _fixedAsset.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code, FixedAssetRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _fixedAsset.Delete(data.Code, _claim.UserId);

            return Ok(result);
        }

        private (bool, string) Validate(FixedAssetRequest data)
        {
            var periods = new List<string> { data.PurchaseDate.ToString("yyyyMM"), data.StartDepreciateOn.ToString("yyyyMM") };
            if (data.OriginalPurchaseDate.HasValue)
                periods.Add(data.OriginalPurchaseDate.Value.ToString("yyyyMM"));
            if (data.OriginalStartDepreciateOn.HasValue)
                periods.Add(data.OriginalStartDepreciateOn.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.PurchaseDate))
                return (false, "Tanggal Perolehan tidak boleh lebih kecil dari tanggal mulai data.");

            // Checking data start date validity
            return !_sysPar.IsStartDateValid(data.StartDepreciateOn)
                ? (false, "Tanggal Mulai Depresiasi tidak boleh lebih kecil dari tanggal mulai data.")
                : (true, "");
        }
    }
}
