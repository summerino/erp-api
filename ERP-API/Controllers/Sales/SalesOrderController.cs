using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model;
using ERP_API.Model.Sales;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/sales-order")]
    //[Authorize]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _so;
        private readonly IUnitOfMeasurementService _uom;

        public SalesOrderController(ISalesOrderService so, IUnitOfMeasurementService uom)
        {
            _so = so;
            _uom = uom;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data = 
                _so.GetData(
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

            var data = _so.GetDetailData(code, fullReceived)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo, x.ItemId, x.ItemInitial, x.ItemName,
                    x.UomId, x.UnitId, x.UnitName, x.Qty,
                    x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                    x.QtyDlv, x.UnitPrice, x.Disc, x.TaxId, x.TaxAmount,
                    x.NettPrice, x.Total, x.Dpp, x.Notes,
                    x.CoaInventory, x.CoaCogs, x.CoaSls, x.CoaSlsDisc, x.CoaSlsReturn,
                    Units = uomC.Where(u => u.UomId == x.UomId)
                        .Select(u => new
                        {
                            u.Id, u.UomId, u.UnitToConvert, u.UnitEquivalent,
                            u.Conversion, u.IsBaseUnit, u.Seq
                        })
                        .OrderBy(u => u.Seq)
                        .ToList(),
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
            var data = _so.GetRelatedTransactions(code);
            
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(SalesOrderRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Insert process
            data.Mark = "A";
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;
            
            var result = _so.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, SalesOrderRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _so.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _so.Delete(code, 1);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesOrderRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate item submitted with same unit.")
                : (true, "");
        }
    }
}
