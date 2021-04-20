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
    [Route("api/v1/supplier")]
    //[Authorize]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplier;

        public SupplierController(ISupplierService supplier)
        {
            _supplier = supplier;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _supplier.GetData(
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
                _supplier.GetLists(
                    null,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Code, x.Initial, x.Name, x.Address1, x.Phone, x.Fax
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("{code}")]
        public IActionResult GetDataByCode(string code)
        {
            return Ok(_supplier.FindByCode(code));
        }

        [HttpPost]
        public IActionResult OnPost(Supplier data)
        {
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _supplier.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, Supplier data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _supplier.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _supplier.Delete(code, 1);

            return Ok(result);
        }
    }
}
