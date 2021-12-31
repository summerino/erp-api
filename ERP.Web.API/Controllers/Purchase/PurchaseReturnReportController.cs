using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("purchase-return-report")]
    [ApiController]
    public class PurchaseReturnReportController : ControllerBase
    {
        private readonly IPurchaseReturnReportService _prp;
        public PurchaseReturnReportController(IPurchaseReturnReportService prp)
        {
            _prp = prp;
        }

        [HttpGet]
        public IActionResult GetData(int type, string startDate, string endDate,
            string supCode, string status, int? itemId,
            string code, bool isDetail)
        {
            var result =
                _prp.GetData(type, startDate, endDate,
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
