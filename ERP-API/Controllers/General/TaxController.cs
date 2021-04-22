using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP_API.Controllers.General
{
    [Route("api/v1/tax")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _taxService;
        private readonly IClaimService _claimService;
        public TaxController(ITaxService taxService, IClaimService claimService)
        {
            _taxService = taxService;
            _claimService = claimService;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _taxService.GetData(
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
        public IActionResult OnPost(Tax data)
        {
            data.IsActive = true;
            data.CreatedBy = _claimService.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _taxService.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, Tax data)
        {
            data.UpdatedBy = _claimService.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _taxService.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _taxService.Delete(id, 1);

            return Ok(result);
        }
    }
}
