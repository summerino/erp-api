using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase;

[Route("purchase-receive-report")]
[ApiController]
public class PurchaseReceiveReportController : ControllerBase
{
    private readonly IPurchaseReceiveReportService _rcv;
    public PurchaseReceiveReportController(IPurchaseReceiveReportService rcv)
    {
        _rcv = rcv;
    }

    [HttpGet]
    public IActionResult GetData(int type, int? srcTrans, string startDate, string endDate,
        string supCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId)
    {
        var result =
            _rcv.GetData(type, srcTrans, startDate, endDate,
                supCode, status, itemId,
                code, isDetail, unitId, categoryId);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}