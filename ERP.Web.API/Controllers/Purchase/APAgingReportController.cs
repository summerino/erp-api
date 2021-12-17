using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("ap-aging-report")]
    [ApiController]
    public class APAgingReportController : ControllerBase
    {
        private readonly IAPAgingReportService _apa;
        public APAgingReportController(IAPAgingReportService apa)
        {
            _apa = apa;
        }

        [HttpGet]
        public IActionResult GetData(int type, string date, string supCode, string duration)
        {
            var result =
                _apa.GetData(type, date, supCode, duration);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
