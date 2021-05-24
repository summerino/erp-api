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
    [Route("api/v1/delivery-plan")]
    [ApiController]
    public class DeliveryPlanController : ControllerBase
    {
        private readonly IDeliveryPlanService _dp;
        private readonly IClaimService _claim;

        public DeliveryPlanController(IDeliveryPlanService deliveryPlan, IClaimService claimService)
        {
            _dp = deliveryPlan;
            _claim = claimService;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _dp.GetData(
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
            var uData = _dp.GetUndeliveredData();
            var data = _dp.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.TransCode,
                    x.Volume,
                    x.Weight,
                    x.SrcTrans,
                    x.IsFailShipment,
                    x.NotesFailShipment,
                    UndeliveredItems = uData.Where(x => x.DlvPlanDetailId == x.Id).OrderBy(x => x.LineNo)
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("related-trans")]
        public IActionResult GetRelatedTransactions(string code)
        {
            var data = _dp.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("all-trans")]
        public IActionResult GetAllTransaction(string code)
        {
            var data = _dp.GetAllTransaction(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(DeliveryPlanRequest data)
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

            var result = _dp.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, DeliveryPlanRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _dp.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _dp.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(DeliveryPlanRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return data.ItemDetails.GroupBy(x => new { x.Code, x.TransCode }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate item submitted with same unit.")
                : (true, "");
        }
    }
}
