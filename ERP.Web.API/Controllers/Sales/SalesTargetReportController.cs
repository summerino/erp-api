using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-target-report")]
[ApiController]
public class SalesTargetReportController : ControllerBase
{
    private readonly ISalesTargetReportService _str;
    public SalesTargetReportController(ISalesTargetReportService str)
    {
        _str = str;
    }

    [HttpGet]
    public IActionResult GetData(string startDate, string endDate,
        int? salesId, int? groupId, int? subGroupId,
        string groupSubGroup, bool isDetail)
    {
        var result =
            _str.GetData(startDate, endDate,
                salesId, groupId, subGroupId,
                groupSubGroup, isDetail);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}