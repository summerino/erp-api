using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;
using System.Linq;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _item;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;

        public ItemController(IItemService item, IUnitOfMeasurementService uom, IClaimService claim)
        {
            _uom = uom;
            _item = item;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category,string warehouseCode, string filters, string sorts, int skip, int take)
        {
            var uomC = _uom.GetDataConversion().ToList();
            var data =
                _item.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    warehouseCode,
                    search);
            var result = (List<VwItem>)data.Data;
            foreach (var item in result)
            {
                if (item.QtyOnHand != null && item.QtyOnIndent != null && item.QtyOnOrder != null && item.QtyOnTransfer != null) {
                    if (item.BuySeq != null)
                    {
                        item.BuyQtyAvailable = (decimal)((item.QtyOnHand - item.QtyOnOrder) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.BuySeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                    }
                    if (item.BuySeq != null)
                    {
                        item.SellQtyAvailable = (decimal)((item.QtyOnHand - item.QtyOnOrder) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.SellSeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                    }
                }
            }
            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = result.ToDynamicList()
            });
        }

        [HttpPost]
        public IActionResult OnPost(Item data)
        {
            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _item.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, Item data)
        {
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _item.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _item.Delete(id, _claim.UserId);

            return Ok(result);
        }
    }
}
