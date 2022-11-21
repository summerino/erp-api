using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("sales-delivery-report")]
[ApiController]
public class SalesDeliveryReportController : ControllerBase
{
    private readonly ISalesDeliveryReportService _sdr;
    public SalesDeliveryReportController(ISalesDeliveryReportService sdr)
    {
        _sdr = sdr;
    }

    [HttpGet]
    public IActionResult GetData(int type, int? srcTrans, string startDate, string endDate,
        int? salesId, string custCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId)
    {
        var result =
            _sdr.GetData(type, srcTrans, startDate, endDate,
                salesId, custCode, status, itemId,
                code, isDetail, unitId, categoryId);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}