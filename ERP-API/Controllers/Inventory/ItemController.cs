using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Services.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Dtos;
using ERP_API.Model;
using Newtonsoft.Json;
using Swift.Framework.Model;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/item")]
    //[Authorize]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _item;
        private ItemService _itemService;

        public ItemController(IItemService item)
        {
            _item = item;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _item.GetData(
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

        [HttpPost]
        public IActionResult OnPost(Item data)
        {
            // Insert process
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _item.Insert(data);

            return Ok(result);
        }

        [HttpPut("{initial}")]
        public IActionResult OnPut(string initial, Item data)
        {
            if (initial != data.Initial)
            {
                if (_itemService.IsInitialExists(data.Initial)) {
                    var existInitial = new SaveResult(false);
                    existInitial.Message = "Initial is already in the database.";
                    return Ok(existInitial);
                }
            }
            // Update process
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _item.Update(data);

            return Ok(result);
        }

        [HttpDelete("{initial}")]
        public IActionResult OnDelete(string Initial)
        {
            var result = _item.Delete(Initial, 1);

            return Ok(result);
        }
    }
}
