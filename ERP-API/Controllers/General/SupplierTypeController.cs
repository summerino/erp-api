using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Newtonsoft.Json;
using Swift.Framework.Model;

namespace ERP_API.Controllers.General
{
    [Route("api/v1/supplier-type")]
    [ApiController]
    public class SupplierTypeController : ControllerBase
    {
        private readonly ISupplierTypeService _supplierType;
        public SupplierTypeController(ISupplierTypeService supplierTypes)
        {
            _supplierType = supplierTypes;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _supplierType.GetData(
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

        [HttpGet("lists")]
        public IActionResult GetList(string sorts)
        {
            var data =
                _supplierType.GetLists(
                    null,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Initial,
                        x.Name
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(SupplierType data)
        {
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _supplierType.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, SupplierType data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _supplierType.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _supplierType.Delete(id, 1);

            return Ok(result);
        }
    }
}
