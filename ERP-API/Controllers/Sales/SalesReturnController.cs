using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

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
        public IActionResult GetDetailData(string code, bool? fullReceived)
        {
            var data = _rtn.GetDetailData(code, fullReceived)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.ItemId,
                    x.ItemInitial,
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

    }
}
