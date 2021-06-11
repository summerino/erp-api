using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Sales;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/promo")]
    [ApiController]
    public class PromoController : ControllerBase
    {
        private readonly IPromoService _promo;
        private readonly IClaimService _claim;
        public PromoController(IPromoService promoService, IClaimService claimService)
        {
            _promo = promoService;
            _claim = claimService;
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
            var result = _promo.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(PromoRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return data.ItemDetails.GroupBy(x => new { x.Code, x.ItemId }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate item submitted with same unit.")
                : (true, "");
        }
    }
}
