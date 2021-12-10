using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("ap-mutation-report")]
    [ApiController]
    public class APMutationReportController : ControllerBase
    {
        private readonly IAPMutationReportService _apm;
        public APMutationReportController(IAPMutationReportService apm)
        {
            _apm = apm;
        }

        [HttpGet]
        public IActionResult GetData(int type, string startDate, string EndDate, string supCode, string status)
        {
            var result =
                _apm.GetData(type, startDate, EndDate, supCode, status);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
