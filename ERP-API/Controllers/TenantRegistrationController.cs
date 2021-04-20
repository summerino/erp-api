using ERP_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_API.Domain.Services;

namespace ERP_API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/Tenant/Registration")]
    public class TenantRegistrationController : ControllerBase
    {
        private readonly IShardingService _shardingService;
        public TenantRegistrationController(IShardingService shardingService)
        {
            _shardingService = shardingService;
        }

        //[HttpPost("New")]
        //public async Task<IActionResult> PostNewTenantAsync(NewTenant newTenant)
        //{
        //    var shardKey = await _shardingService.CreateNewInstanceAsync(newTenant.TenantName);
        //    return Ok(shardKey);
        //}
    }
}
