using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.MobileSales;

[Route("mobile-activity-log-report")]
[ApiController]
public class MobileActivityLogReportController : ControllerBase
{
    private readonly IMobileActivityLogReportService _mal;

    public MobileActivityLogReportController(IMobileActivityLogReportService mal)
    {
        _mal = mal;
    }

    [HttpGet]
    public IActionResult GetData(string username, string typeCode, string note, string startDate, string endDate)
    {
        var result = _mal.GetData(username, typeCode, note, startDate, endDate);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}