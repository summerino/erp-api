using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase;

[Route("purchase-invoice-report")]
[ApiController]
public class PurchaseInvoiceReportController : ControllerBase
{
    private readonly IPurchaseInvoiceReportService _pir;
    public PurchaseInvoiceReportController(IPurchaseInvoiceReportService pir)
    {
        _pir = pir;
    }

    [HttpGet]
    public IActionResult GetData(int type, string startDate, string endDate,
        string supCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId)
    {
        var result =
            _pir.GetData(type, startDate, endDate,
                supCode, status, itemId,
                code, isDetail, unitId,
                categoryId);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}