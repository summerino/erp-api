using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("release-overlimit-report")]
    [ApiController]
    public class ReleaseOverlimitReportController : ControllerBase
    {
        readonly IReleaseOverlimitReportService _ror;
        public ReleaseOverlimitReportController(IReleaseOverlimitReportService ror)
        {
            _ror = ror;
        }

        [HttpGet]
        public IActionResult GetData(string startDate, string endDate, string releasedBy, string custCode, string sorts)
        {
            var result =
                _ror.GetData(startDate, endDate, releasedBy, custCode,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
