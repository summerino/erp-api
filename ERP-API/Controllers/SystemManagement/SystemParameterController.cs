using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;

namespace ERP_API.Controllers.SystemManagement
{
    [Route("api/v1/system-parameter")]
    [ApiController]
    public class SystemParameterController : ControllerBase
    {
        private readonly ISystemParameterService _sysParam;
        private readonly IClaimService _claim;

        public SystemParameterController(ISystemParameterService sysParam, IClaimService claimService)
        {
            _sysParam = sysParam;
            _claim = claimService;
        }

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts) 
        {
            var data =
                _sysParam.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Code, x.Value
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
