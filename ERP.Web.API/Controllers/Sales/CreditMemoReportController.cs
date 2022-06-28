using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales;

[Route("credit-memo-report")]
[ApiController]
public class CreditMemoReportController : ControllerBase
{
    private readonly ICreditMemoReportService _cmr;

    public CreditMemoReportController(ICreditMemoReportService cmr)
    {
        _cmr = cmr;
    }

    [HttpGet]
    public IActionResult GetData(int type, string custCode, string date, string status, string sorts)
    {
        var result =
            _cmr.GetData(
                type, date, custCode, status,
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}