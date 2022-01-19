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
using ERP.Web.API.Domain.Interfaces.Accounting;

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
        private readonly IClosingMonthService _closingMonth;
        private const int MenuId = (int)Menu.Promo;

        public PromoController(IPromoService promo, ISystemParameterService sysPar, IClaimService claim, IAuthService auth, IClosingMonthService closingMonthService)
        {
            _promo = promo;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
            _closingMonth = closingMonthService;
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
            var multiData = _promo.GetMultipleItemsData();
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
                    PromoTierList = tierData.Where(y => y.PromoDetailId == x.Id),
                    MultipleItem = multiData.Where(z => z.PromoDetailId == x.Id)
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("subject")]
        public IActionResult GetSubjectData(string code)
        {
            var data = _promo.GetSubjectData(code).ToList<dynamic>();

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
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
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
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
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
        public IActionResult OnDelete(string code, PromoRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data, true);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _promo.Delete(data.Code, _claim.UserId);

            return Ok(result);
        }

        [HttpGet("list")]
        public IActionResult GetListPromo(string code)
        {
            var data = _promo.GetListPromo(code).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        private (bool, string) Validate(PromoRequest data, bool onDelete = false)
        {
            var periods = new List<string> { data.StartDate.ToString("yyyyMM"), data.EndDate.ToString("yyyyMM") };
            if (data.OriginalStartDate.HasValue)
                periods.Add(data.OriginalStartDate.Value.ToString("yyyyMM"));
            if (data.OriginalEndDate.HasValue)
                periods.Add(data.OriginalEndDate.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.StartDate))
                return (false, "Tanggal Mulai tidak boleh lebih kecil dari tanggal mulai data.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.EndDate))
                return (false, "Tanggal Akhir tidak boleh lebih kecil dari tanggal mulai data.");

            if (!onDelete)
            {
                if (!data.ItemDetails.Any())
                    return (false, "Detail tidak boleh kosong.");

                if (data.ItemDetails.Where(x => x.ApplyTo != 2).GroupBy(x => new { x.Code, x.ItemId, x.PromoType }).Any(x => x.Count() > 1))
                    return(false, "Terdapat data detail yang sama.");
            }

            return (true, "");
        }
    }
}
