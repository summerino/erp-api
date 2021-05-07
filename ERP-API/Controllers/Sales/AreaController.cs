using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Model;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/area")]
    //[Authorize]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly IAreaService _area;
        private readonly IClaimService _claim;

        public AreaController(IAreaService area)
        {
            _area = area;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _area.GetData(
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

        [HttpGet("hierarchy")]
        public IActionResult GetHierarchy()
        {
            return Ok(_area.GetHierarchy());
        }

        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _area.GetLists()
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.Name,
                    x.ParentId,
                    x.Deep,
                    x.Lineage
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(Area data)
        {
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _area.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(int id, Area data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _area.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _area.Delete(id, 1);

            return Ok(result);
        }
    }
}
