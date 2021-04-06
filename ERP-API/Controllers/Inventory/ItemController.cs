using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
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

        public ItemController(IItemService item)
        {
            _item = item;
        }

        [HttpGet]
        public IActionResult GetData(string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _item.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"));

            return Ok(new MasterViewDto
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }
    }
}
