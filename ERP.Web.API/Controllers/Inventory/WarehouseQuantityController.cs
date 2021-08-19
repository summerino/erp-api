using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("warehouse-quantity")]
    [ApiController]
    public class WarehouseQuantityController : ControllerBase
    {
        private readonly IWarehouseQuantityService _warehouseQuantity;
        private readonly IClaimService _claim;

        public WarehouseQuantityController(IWarehouseQuantityService warehouseQuantity, IClaimService claim)
        {
            _warehouseQuantity = warehouseQuantity;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _warehouseQuantity.GetData(
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
    }
}
