using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.MobileSales;

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
    public IActionResult GetData(string startDate, string endDate, int? salesId)
    {
        var result = _mvp.GetData(startDate, endDate, salesId).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = result.Count,
            TableData = result
        });
    }
}