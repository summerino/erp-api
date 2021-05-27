using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.General;
using Newtonsoft.Json;

namespace ERP_API.Controllers.General
{
    [Route("api/v1/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customer;
        private readonly IClaimService _claim;

        public CustomerController(ICustomerService customer, IClaimService claim)
        {
            _customer = customer;
            _claim = claim;
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

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts) 
        {
            var data =
                _customer.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
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

        [HttpGet("addresses")]
        public IActionResult GetAddress(string code)
        {
            var data =
                _customer.GetAddress(code)
                    .Select(x => new
                    {
                        x.Id,
                        x.Code,
                        x.Initial,
                        x.Address1,
                        x.Address2,
                        x.ContactPerson,
                        x.Phone,
                        x.Fax,
                        x.IsDefault
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
            return Ok(_customer.FindByCode(code));
        }

        [HttpPost]
        public IActionResult OnPost(CustomerRequest data)
        {
            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _customer.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, CustomerRequest data)
        {
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _customer.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _customer.Delete(code, _claim.UserId);

            return Ok(result);
        }
    }
}
