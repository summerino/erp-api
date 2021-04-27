using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Inventory;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/item-group")]
    [ApiController]
    public class ItemGroupController : ControllerBase
    {
        private readonly IItemGroupService _itemGroup;

        public ItemGroupController(IItemGroupService itemGroup)
        {
            _itemGroup = itemGroup;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _itemGroup.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    null,
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("item")]
        public IActionResult GetDetailData(int id)
        {
            var data = _itemGroup.GetDetailData(id)
                .Select(x => new
                {
                    x.Id,
                    x.ItemGroupId,
                    x.Name,
                    x.Value
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(ItemGroupRequest data)
        {
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _itemGroup.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(int id, ItemGroupRequest data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _itemGroup.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _itemGroup.Delete(id, 1);

            return Ok(result);
        }
    }
}
