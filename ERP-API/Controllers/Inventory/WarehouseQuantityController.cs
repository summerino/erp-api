using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP.Entity.Inventory;
using ERP_API.Domain.Models;
using ERP.Entity;
using ERP_API.Model;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Inventory
{
    [Route("warehouse-quantity")]
    [ApiController]
    public class WarehouseQuantityController : ControllerBase
    {
        private readonly IWarehouseQuantityService _warehouse_quantity;
        private readonly IClaimService _claim;

        public WarehouseQuantityController(IWarehouseQuantityService warehouse_quantity, IClaimService claim)
        {
            _warehouse_quantity = warehouse_quantity;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _warehouse_quantity.GetData(
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
