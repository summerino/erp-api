using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Finance;

[Route("outstanding-cheque-report")]
[ApiController]
public class OutstandingChequeReportController : ControllerBase
{
    private readonly IOutstandingChequeReportService _ocr;
    public OutstandingChequeReportController(IOutstandingChequeReportService ocr)
    {
        _ocr = ocr;
    }

    [HttpGet]
    public IActionResult GetData(string date, string coaCode)
    {
        var result =
            _ocr.GetData(date, coaCode);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}