using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-down-payment-report")]
[ApiController]
public class SalesDownPaymentReportController : ControllerBase
{
    private readonly ISalesDownPaymentReportService _sdpr;

    public SalesDownPaymentReportController(ISalesDownPaymentReportService sdpr)
    {
        _sdpr = sdpr;
    }

    [HttpGet]
    public IActionResult GetData(int type, int? srcTrans, string custCode, string date, string status, string sorts)
    {
        var result =
            _sdpr.GetData(
                type, srcTrans, date, custCode, status,
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}