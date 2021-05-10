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
    [Route("api/v1/sales-return")]
    [ApiController]
    public class SalesReturnController : ControllerBase
    {
        private readonly ISalesReturnService _rtn;
        private readonly IClaimService _claim;

        public SalesReturnController(ISalesReturnService rtn, IClaimService claim)
        {
            _rtn = rtn;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _rtn.GetData(
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
        public IActionResult GetDetailData(string code, bool? fullDelivered)
        {
            var data = _rtn.GetDetailData(code, fullDelivered)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.TransDetailId,
                    x.ItemId,
                    x.ItemName,
                    x.UomId,
                    x.UnitId,
                    x.UnitName,
                    x.Qty,
                    x.QtyDlv,
                    x.Length,
                    x.Width,
                    x.Height,
                    x.Weight,
                    x.DimensionMeasurement,
                    x.WeightMeasurement,
                    x.UnitPrice,
                    x.Disc,
                    x.TaxId,
                    x.TaxAmount,
                    x.NettPrice,
                    x.Total,
                    x.Dpp,
                    OldUnitId = x.ItemUomSellId,
                    OldUnitName = x.ItemUomSellName,
                    OldUnitPrice = x.ItemSellPrice,
                    TotTax = x.Qty * x.TaxAmount,
                    TotDPP = x.Qty * x.Dpp,
                    State = ""
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
            var data = _rtn.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(SalesReturnRequest data)
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

            var result = _rtn.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, SalesReturnRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _rtn.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _rtn.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesReturnRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
                return (false, "There are duplicate item submitted with same unit.");

            return data.ItemDetails.Sum(x => x.Qty) <= 0
                ? (false, "Total receive qty can't be 0.")
                : (true, "");
        }
    }
}
