using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using ERP_API.Domain.Entities.General;
using ERP_API.Model.Accounting;

namespace ERP_API.Controllers.Accounting
{
    [Route("api/v1/currency-rate")]
    [AllowAnonymous]
    [ApiController]
    public class CurrencyRateController : ControllerBase
    {
        private readonly ICurrencyRateService _currencyRate;
        private readonly IClaimService _claim;

        public CurrencyRateController(ICurrencyRateService currencyRate, IClaimService claim)
        {
            _currencyRate = currencyRate;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _currencyRate.GetData(
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
        public IActionResult OnPost(CurrencyRateRequest data)
        {
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _currencyRate.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, CurrencyRate data)
        {
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;
            var result = _currencyRate.Update(data);
            return Ok(result);
        }

        [HttpGet]
        [Route("getlistcurrencies")]
        public IActionResult GetListCurrencies() 
        {
            var result = new List<Currency>();
            result.Add(new Currency { Code = "USD", Name = "US Dollar", Sort = 2, IsActive = true });
            return Ok(result);
        }
    }
}
