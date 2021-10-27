using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.Mobile.CustomerTransaction
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class CustomerTransactionController : ControllerBase
    {
        private readonly ICustomerTransactionService _customerTransaction;

        public CustomerTransactionController(ICustomerTransactionService customerTransaction)
        {
            _customerTransaction = customerTransaction;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, int skip, int take, DateTime? date)
        {
            var custCode = "C000001";
            var data =
                _customerTransaction.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),date,custCode);

            
            var result = data.Data.ToDynamicList().Select(x => new
            {
                x.Code,
                x.Date,
                x.Total,
                Status=x.Remaining==0?"Lunas":"Belum"
            }).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result.ToDynamicList()
            });
        }

        [HttpGet("items")]
        public IActionResult GetTransactionItem(string code)
        {
            var data =
                _customerTransaction.GetTransactionItem(code);
            return Ok(data);
        }

        [HttpGet("credit-limit")]
        public IActionResult GetCreditLimit()
        {
            var custCode = "C000001";
            var data =
                _customerTransaction.GetCreditLimit(custCode);
            return Ok(data);
        }
    }
}
