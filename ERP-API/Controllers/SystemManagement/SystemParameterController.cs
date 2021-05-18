using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP_API.Controllers.SystemManagement
{
    [Route("api/v1/user")]
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

        [HttpGet("item")]
        public IActionResult GetDetail(string[] codes)
        {
            var param = _sysParam.GetList(codes);
            var data = param.ToList<dynamic>();
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
    }
}
