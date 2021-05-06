using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Purchase;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Purchase
{
    [Route("api/v1/purchase-order")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _po;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;

        public PurchaseOrderController(IPurchaseOrderService po, IUnitOfMeasurementService uom, IClaimService claim)
        {
            _po = po;
            _uom = uom;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data = 
                _po.GetData(
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
        public IActionResult GetDetailData(string code, bool? fullReceived)
        {
            var uomC =_uom.GetDataConversion().ToList();

            var data = _po.GetDetailData(code, fullReceived)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo, x.ItemId, x.ItemName, x.UomId, x.UnitId, x.UnitName, x.Qty,
                    x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                    x.QtyRcv, x.UnitPrice, x.Disc, x.TaxId, x.TaxAmount,
                    x.NettPrice, x.Total, x.Dpp, x.Notes,
                    x.CoaInventory, x.CoaCogs, x.CoaPurc, x.CoaPurcDisc, x.CoaPurcReturn, x.Type,
                    Units = uomC.Where(u => u.UomId == x.UomId)
                        .Select(u => new
                        {
                            u.Id, u.UomId, u.UnitToConvert, u.UnitEquivalent,
                            u.Conversion, u.IsBaseUnit, u.Seq
                        })
                        .OrderBy(u => u.Seq)
                        .ToList(),
                    OldUnitId = x.ItemUomBuyId,
                    OldUnitName = x.ItemUomBuyName,
                    OldUnitPrice = x.ItemBuyPrice,
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
            var data = _po.GetRelatedTransactions(code);
            
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(PurchaseOrderRequest data)
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
            
            var result = _po.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, PurchaseOrderRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _po.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _po.Delete(code, _claim.UserId);

            return Ok(result);
        }

        [HttpDelete("close/{code}")]
        public IActionResult OnClose(string code)
        {
            var result = _po.Close(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(PurchaseOrderRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate item submitted with same unit.")
                : (true, "");
        }
    }
}
