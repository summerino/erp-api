using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Model;

namespace ERP_API.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
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
