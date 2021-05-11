using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;
using ERP_API.Model.Inventory;
using ERP_API.Domain.Interfaces.Inventory;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/adjustment")]
    [ApiController]

    public class AdjustmentController : ControllerBase
    {
        private readonly IAdjustmentService _adjustment;
        private readonly IClaimService _claim;
        private readonly IUnitOfMeasurementService _uom;


        public AdjustmentController(IAdjustmentService adjustment, IClaimService claim, IUnitOfMeasurementService uom)
        {
            _adjustment = adjustment;
            _claim = claim;
            _uom = uom;
        }
        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {

            var data =
                _adjustment.GetData(
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

            var data = _adjustment.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.ItemId,
                    x.ItemName,
                    x.Notes,
                    x.QtyAdjust,
                    x.QtyOpname,
                    x.Different,
                    x.QtyOnHand,
                    x.BaseQtyOnHand,
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
                    x.UnitId,
                    OldUnitId = x.UnitId,
                    x.UomId,
                    State = ""
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("item-list")]
        public IActionResult GetItemForAdjustment(string search, string category, string filters, string sorts, int skip, int take) 
        {
            var data =
                _adjustment.GetAdjustmentItem(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }
        [HttpPost]
        public IActionResult OnPost(AdjustmentRequest data)
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

            var result = _adjustment.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, AdjustmentRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _adjustment.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _adjustment.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(AdjustmentRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return (true, "");
        }
    }
}
