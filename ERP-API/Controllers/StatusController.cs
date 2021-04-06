using ERP_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ERP_API.Controllers
{
    [AllowAnonymous]
    [Route("api/v1/status")]
    public class StatusController : Controller
    {
        public StatusController() { }

        [HttpGet]
        public async Task<ApiStatus> GetStatus()
        {
            ApiStatus status = await Task.Run(() => new ApiStatus
            {
                Status = "Running",
                Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
                UtcTime = DateTime.UtcNow
            });

            return status;
        } 
    }
}
