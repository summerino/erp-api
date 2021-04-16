using System;
using System.Collections.Generic;
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
    [Route("api/v1/customer")]
    //[Authorize]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomersService _customer;

        public CustomerController(ICustomersService customer)
        {
            _customer = customer;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _customer.GetData(
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
        public IActionResult OnPost(Customer data)
        {
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _customer.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, Customer data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _customer.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _customer.Delete(code, 1);

            return Ok(result);
        }
    }
}
