using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("purchase-order-report")]
    [ApiController]
    public class PurchaseOrderReportController : ControllerBase
    {
        private readonly IPurchaseOrderReportService _por;
        public PurchaseOrderReportController(IPurchaseOrderReportService por)
        {
            _por = por;
        }

        [HttpGet]
        public IActionResult GetData(int type, string startDate, string endDate,
            string supCode, string status, int? itemId,
            string code, bool isDetail)
        {
            var result =
                _por.GetData(type, startDate, endDate,
                    supCode, status, itemId,
                    code, isDetail);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
