using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("ap-card-report")]
    [ApiController]
    public class APCardReportController : ControllerBase
    {
        private readonly IAPCardReportService _apc;
        public APCardReportController(IAPCardReportService apc)
        {
            _apc = apc;
        }

        [HttpGet]
        public IActionResult GetData(string startDate, string endDate, string supCode)
        {
            var result =
                _apc.GetData(startDate, endDate, supCode);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
