using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileSales
{
    [Route("mobile-visit-performance-report")]
    [ApiController]
    public class MobileVisitPerformanceReportController : ControllerBase
    {
        private readonly IMobileVisitPerformanceReportService _mvp;
        public MobileVisitPerformanceReportController(IMobileVisitPerformanceReportService mvp)
        {
            _mvp = mvp;
        }

        [HttpGet]
        public IActionResult GetData(int? salesId, string startDate, string endDate)
        {
            var result = _mvp.GetData(salesId, startDate, endDate);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
