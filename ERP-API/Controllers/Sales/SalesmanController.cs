using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales
{    
    [Route("salesman")]
    [ApiController]
    public class SalesmanController : ControllerBase
    {
        private readonly ISalesmanService _salesman;
        private readonly IClaimService _claim;

        public SalesmanController(ISalesmanService salesman, IClaimService claim)
        {
            _salesman = salesman;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _salesman.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("salesman-schedule")]
        public IActionResult GetEmployeeSchedule(string groupId, string startDate, string recurrence, string visitDay)
        {
            var data =
                _salesman.GetSalesmanSchedule(groupId, startDate, recurrence, visitDay).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("salesman-schedule-by-id")]
        public IActionResult GetEmployeeSchedule(long id)
        {
            var data =
                _salesman.GetSalesmanSchedule(id).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("salesman-schedule-customer")]
        public IActionResult GetEmployeeScheduleDetailData(string ids)
        {
            var data = _salesman.GetSalesmanScheduleDetailData(JsonConvert.DeserializeObject<List<long>>(!string.IsNullOrWhiteSpace(ids) ? ids : "[]")).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
    }
}
