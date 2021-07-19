using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("[controller]")]
    [ApiController]
    public class PromoController : ControllerBase
    {
        private readonly IPromoService _promo;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int _menuId = (int)Menu.Promo;

        public PromoController(IPromoService promo, ISystemParameterService sysPar, IClaimService claim, IAuthService auth)
        {
            _promo = promo;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _promo.GetData(
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
            var tierData = _promo.GetDetailTierData();
            var data = _promo.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.ApplyTo,
                    x.ItemId,
                    x.PromoType,
                    x.IsPercentage,
                    x.ValuePercentage,
                    x.ValueAmount,
                    x.IsPromoWithBudget,
                    x.BudgetMaximumValue,
                    x.OverBudgetAction,
                    PromoTierList = tierData.Where(y => y.PromoDetailId == x.Id)
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(PromoRequest data)
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

            var result = _promo.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(PromoRequest data)
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

            var result = _promo.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            var result = _promo.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private (bool, string) Validate(PromoRequest data)
        {
            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.StartDate))
                return (false, "Tanggal Mulai tidak boleh lebih kecil dari tanggal mulai data.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.EndDate))
                return (false, "Tanggal Akhir tidak boleh lebih kecil dari tanggal mulai data.");

            if (!data.ItemDetails.Any())
                return (false, "Detail tidak boleh kosong.");

            return data.ItemDetails.Where(x => x.ApplyTo != 2).GroupBy(x => new { x.Code, x.ItemId, x.PromoType }).Any(x => x.Count() > 1)
                ? (false, "Terdapat data detail yang sama.")
                : (true, "");
        }
    }
}
