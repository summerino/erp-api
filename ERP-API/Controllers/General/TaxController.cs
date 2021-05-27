using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;

namespace ERP_API.Controllers.General
{
    [Route("api/v1/tax")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _tax;
        private readonly IClaimService _claim;

        public TaxController(ITaxService tax, IClaimService claim)
        {
            _tax = tax;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _tax.GetData(
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
                _tax.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id, x.Initial, x.Name, x.Rate
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(Tax data)
        {
            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _tax.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, Tax data)
        {
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _tax.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _tax.Delete(id, _claim.UserId);

            return Ok(result);
        }
    }
}
