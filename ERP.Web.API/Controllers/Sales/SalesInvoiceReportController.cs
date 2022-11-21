using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-invoice-report")]
[ApiController]
public class SalesInvoiceReportController : ControllerBase
{
    private readonly ISalesInvoiceReportService _sir;
    public SalesInvoiceReportController(ISalesInvoiceReportService sir)
    {
        _sir = sir;
    }

    [HttpGet]
    public IActionResult GetData(int type, string startDate, string endDate,
        int? salesId, string custCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId)
    {
        var result =
            _sir.GetData(type, startDate, endDate,
                salesId, custCode, status, itemId,
                code, isDetail, unitId,
                categoryId);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}