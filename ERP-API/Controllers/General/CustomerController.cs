using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Dtos;
using ERP_API.Model;
using Newtonsoft.Json;
using ERP_API.Domain.Entities.General;
using Swift.Framework.Model;

namespace ERP_API.Controllers.General
{   
    [Route("api/v1/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomersService _cs;
        public CustomerController(ICustomersService customersService)
        {
            _cs = customersService;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, int skip, int take)
        {
            var data =
                _cs.GetData<Customer>(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new MasterViewDto
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("{code}")]
        public IActionResult GetCustomerDetail(string code)
        {
            var data = _cs.GetCustomers(code);
            return Ok(new MasterAddEditDto
            {
                TableData = data
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

            var result = _cs.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code,[FromBody]Customer data)
        {

            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _cs.Update(data, 1);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _cs.Delete(code, 1);

            return Ok(result);
        }
    }
}
