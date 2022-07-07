using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("ar-card-report")]
    [ApiController]
    public class ARCardReportController : ControllerBase
    {
        private readonly IARCardReportService _arc;
        public ARCardReportController(IARCardReportService arc)
        {
            _arc = arc;
        }

        [HttpGet]
        public IActionResult GetData(string startDate, string endDate, string CustCode, int? salesId)
        {
            var result =
                _arc.GetData(startDate, endDate, CustCode, salesId);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
