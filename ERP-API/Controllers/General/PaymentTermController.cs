using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
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
    [Route("api/v1/payment-term")]
    [ApiController]
    public class PaymentTermController : ControllerBase
    {
        private readonly IPaymentTermService _paymentTerm;
        
        public PaymentTermController(IPaymentTermService paymentTermService)
        {
            _paymentTerm = paymentTermService;
        }

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts)
        {
            var data =
                _paymentTerm.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
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
    }
}
