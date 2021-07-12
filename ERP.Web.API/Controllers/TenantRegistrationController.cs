using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Services;

namespace ERP.Web.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("cTenant/Registration")]
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
