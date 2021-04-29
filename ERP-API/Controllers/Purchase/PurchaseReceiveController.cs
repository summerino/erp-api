using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Purchase;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Purchase
{
    [Route("api/v1/purchase-receive")]
    [ApiController]
    public class PurchaseReceiveController : ControllerBase
    {
        private readonly IPurchaseReceiveService _rcv;
        private readonly IClaimService _claim;

        public PurchaseReceiveController(IPurchaseReceiveService rcv, IClaimService claim)
        {
            _rcv = rcv;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _rcv.GetData(
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
            var data = _rcv.GetDetailData(code)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo,
                    PoDetailId = x.TransDetailId, x.ItemId, x.ItemInitial, x.ItemName,
                    x.OrderQty, x.OutstandingQty, x.Qty,
                    x.UomId, x.UnitId, x.UnitName,
                    x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                    x.UnitPrice, x.Disc, x.TaxId, x.TaxAmount, x.NettPrice, x.Total, x.Dpp,
                    x.WarehouseCode, x.Type,
                    OldUnitId = x.ItemUomBuyId,
                    OldUnitName = x.ItemUomBuyName,
                    OldUnitPrice = x.ItemBuyPrice,
                    TotTax = x.Qty * x.TaxAmount,
                    TotDPP = x.Qty * x.Dpp,
                    TypeName = x.Type == 0 ? "Normal" : "Bonus",
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
            var data = _rcv.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("un-invoice")]
        public IActionResult GetUnInvoiceData(string poCode, string invCode)
        {
            var data = _rcv.GetUnInvoiceData(poCode, invCode).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(PurchaseReceiveRequest data)
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

            var result = _rcv.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, PurchaseReceiveRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _rcv.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _rcv.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(PurchaseReceiveRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId, x.Type }).Any(x => x.Count() > 1))
                return (false, "There are duplicate item submitted with same unit.");

            return data.ItemDetails.Where(x => x.Type == 0).Sum(x => x.Qty) <= 0
                ? (false, "Total receive qty can't be 0.")
                : (true, "");
        }
    }
}
