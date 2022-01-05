using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("sales-order-report")]
    [ApiController]
    public class SalesOrderReportController : ControllerBase
    {
        private readonly ISalesOrderReportService _sor;
        public SalesOrderReportController(ISalesOrderReportService sor)
        {
            _sor = sor;
        }

        [HttpGet]
        public IActionResult GetData(int type, string startDate, string endDate,
            string custCode, string status, int? itemId,
            string code, bool isDetail, int? unitId,
            int? categoryId)
        {
            var result =
                _sor.GetData(type, startDate, endDate,
                    custCode, status, itemId,
                    code, isDetail, unitId,
                    categoryId);

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
