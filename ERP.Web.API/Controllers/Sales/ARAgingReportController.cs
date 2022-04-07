using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("ar-aging-report")]
[ApiController]
public class ARAgingReportController : ControllerBase
{
    private readonly IARAgingReportService _ara;
    public ARAgingReportController(IARAgingReportService ara)
    {
        _ara = ara;
    }

    [HttpGet]
    public IActionResult GetData(int type, string date, string custCode, int? slsId, string duration)
    {
        var result =
            _ara.GetData(type, date, custCode, slsId, duration);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}