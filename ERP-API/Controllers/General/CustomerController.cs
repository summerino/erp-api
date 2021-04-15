using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Model;
using Newtonsoft.Json;
using Swift.Framework.Model;
using ERP_API.Domain.Models;

namespace ERP_API.Controllers.General
{
    [Route("api/v1/customer")]
    //[Authorize]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomersService _cust;

        public CustomerController(ICustomersService cust)
        {
            _cust = cust;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cust.GetViewData(
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
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _cust.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, Customer data)
        {
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _cust.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _cust.Delete(code, 1);

            return Ok(result);
        }

        private static (bool, string) Validate(Customer data)
        {
            if (data.Initial == null || data.Initial == "")
                return (false, "Initial Code cannot be empty.");

            if (data.TypeId == 0)
                return (false, "Type Id Code cannot be empty.");

            if (data.Name == null || data.Name == "")
                return (false, "Name cannot be empty.");

            if (data.Address1 == null || data.Address1 == "")
                return (false, "Address Code cannot be empty.");

            if (data.Phone == null || data.Phone == "")
                return (false, "Phone Code cannot be empty.");

            return (true, "");
        }
    }
}
