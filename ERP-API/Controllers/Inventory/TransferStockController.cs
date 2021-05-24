using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Inventory;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/transfer-stock")]
    [ApiController]
    public class TransferStockController : ControllerBase
    {
        private readonly ITransferStockService _ts;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;

        public TransferStockController(ITransferStockService ts, IUnitOfMeasurementService uom, IClaimService claim)
        {
            _ts = ts;
            _uom = uom;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _ts.GetData(
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
            var uomC = _uom.GetDataConversion().ToList();

            var data = _ts.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.ItemId,
                    x.ItemName,
                    x.UomId,
                    x.UnitId,
                    x.Qty,
                    Units = uomC.Where(u => u.UomId == x.UomId)
                        .Select(u => new
                        {
                            u.Id,
                            u.UomId,
                            u.UnitToConvert,
                            u.UnitEquivalent,
                            u.Conversion,
                            u.IsBaseUnit,
                            u.Seq
                        })
                        .OrderBy(u => u.Seq)
                        .ToList(),
                    x.Notes,
                    x.UnitName
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
            var data = _ts.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(TransferStockRequest data)
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

            var result = _ts.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, TransferStockRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _ts.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _ts.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(TransferStockRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Detil barang tidak boleh kosong.");

            return data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1)
                ? (false, "Ada barang yang sama dalam satuan yang sama.")
                : (true, "");
        }
    }
}
