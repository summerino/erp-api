using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Sales;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/visit-plan")]
    [ApiController]
    public class VisitPlanController : ControllerBase
    {
        private readonly IVisitPlanService _visitPlan;
        private readonly IClaimService _claim;

        public VisitPlanController(IVisitPlanService visitPlan, IClaimService claim)
        {
            _visitPlan = visitPlan;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _visitPlan.GetData(
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

        [HttpGet("visit-plan-detail")]
        public IActionResult GetDetailData(string code)
        {
            var data = _visitPlan.GetDetailData(code).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("visit-plan-detail-customer")]
        public IActionResult GetCustomerDetailData(string ids)
        {
            var data = _visitPlan.GetCustomerDetailData(JsonConvert.DeserializeObject<List<long>>(!string.IsNullOrWhiteSpace(ids) ? ids : "[]")).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(VisitPlanRequest data)
        {
            data.Mark = "A";
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _visitPlan.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, VisitPlanRequest data)
        {
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _visitPlan.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            var result = _visitPlan.Delete(code, _claim.UserId);

            return Ok(result);
        }
    }
}
