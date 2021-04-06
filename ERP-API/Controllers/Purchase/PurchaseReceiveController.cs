using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Dtos;
using ERP_API.Model.Purchase;
using Newtonsoft.Json;
using Swift.Framework.Model;

namespace ERP_API.Controllers.Purchase
{
    [Route("api/v1/purchase-receive")]
    //[Authorize]
    [ApiController]
    public class PurchaseReceiveController : ControllerBase
    {
        private readonly IPurchaseReceiveService _rcv;
        private readonly IUoMConversionService _uomC;

        public PurchaseReceiveController(IPurchaseReceiveService rcv, IUoMConversionService uomC)
        {
            _rcv = rcv;
            _uomC = uomC;
        }

        [HttpGet]
        public IActionResult GetData(string sorts, int skip, int take) 
        {
            var sortLists = !string.IsNullOrWhiteSpace(sorts)
                ? JsonConvert.DeserializeObject<List<Sort>>(sorts)
                : null;

            var data = _rcv.GetData(skip, take, null, sortLists);
            
            return Ok(new MasterViewDto
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("item")]
        public IActionResult GetDetailData(string code)
        {
            var uomC =_uomC.GetData().ToList();

            var data = _rcv.GetDetailData(code)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo, x.ItemId, x.ItemName, x.UomId, x.UnitId, x.UnitName, x.Qty,
                    x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                    x.QtyRcv, x.UnitPrice, x.Disc, x.IncludeTax, x.TaxId, x.TaxAmount,
                    x.NettPrice, x.Total, x.Dpp, x.Notes,
                    x.CoaInventory, x.CoaCogs, x.CoaPurc, x.CoaPurcDisc, x.CoaPurcReturn, x.Type,
                    units = uomC.Where(u => u.UomId == x.UomId)
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

            return Ok(new MasterViewDto
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
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _rcv.Insert(data);

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
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _rcv.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _rcv.Delete(code, 1);

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
