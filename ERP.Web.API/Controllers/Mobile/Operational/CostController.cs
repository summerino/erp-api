using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileSales;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.Operational
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class CostController : ControllerBase
    {
        private readonly IMobileCostService _mobCost;
        private readonly ICoaService _coa;
        private readonly IClaimService _claim;

        public CostController(IMobileCostService mobCost, ICoaService coa, IClaimService claim)
        {
            _mobCost = mobCost;
            _coa = coa;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, int skip, int take, string date)
        {
            var data =
                _mobCost.GetDataForMobile(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), _claim.UserId, date);

            var result = ((List<MobileCostHeader>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            });
        }

        [HttpPost]
        public IActionResult AddCost(MobileCostRequest data)
        {
            // Insert process
            data.Mark = "A";
            data.Rate = 1;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _mobCost.Insert(data);
            return Ok(result);
        }

        [HttpGet("detail")]
        public IActionResult GetDetail(string code)
        {
            var data = _mobCost.GetDetailData(code)
                .Select(x => new
                {
                    x.Code, x.LineNo,
                    x.CoaCode, x.CoaName,
                    x.Amount
                });

            return Ok(data);
        }

        [HttpGet("coa")]
        public IActionResult GetMobileCoa(string lastUpdate)
        {
            var data =
                _coa.GetLists(
                    new List<Filter>
                    {
                        new() {Field = "ShowInMobile", Operator = "eq", Keyword = "1"}
                    },
                    null,
                    lastUpdate).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                       x.Id, x.Code,  x.Name, x.IsActive, x.UpdatedDate
                    });

            return Ok(data);
        }

        [HttpGet("image")]
        public IActionResult GetImage(string code)
        {
            var data = _mobCost.GetImageData(code)
                .Select(x => new
                {
                    x.Code, x.LineNo, x.Image
                });

            return Ok(data);
        }

        [HttpGet("today")]
        public IActionResult GetTodayTransaction(string date)
        {
            return Ok(new
            {
                Exist = _mobCost.IsCostExists(date, _claim.UserId)
            });
        }
    }
}
