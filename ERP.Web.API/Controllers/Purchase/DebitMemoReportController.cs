using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Purchase;

[Route("debit-memo-report")]
[ApiController]
public class DebitMemoReportController : ControllerBase
{
    private readonly IDebitMemoReportService _dm;

    public DebitMemoReportController(IDebitMemoReportService dm)
    {
        _dm = dm;   
    }

    [HttpGet]
    public IActionResult GetData(int type, string supCode, string date, string status, string sorts)
    {
        var result =
            _dm.GetData(
                type, date, supCode, status,
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}