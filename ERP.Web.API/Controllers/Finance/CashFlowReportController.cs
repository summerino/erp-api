using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("cash-flow-report")]
    [ApiController]
    public class CashFlowReportController : ControllerBase
    {
        private readonly ICashFlowReportService _cfr;

        public CashFlowReportController(ICashFlowReportService cfr)
        {
            _cfr = cfr;
        }

        [HttpGet]
        public IActionResult GetData(int type, string dateFrom, string dateTo, string coaCode)
        {
            if (string.IsNullOrEmpty(coaCode))
            {
                var result = _cfr.GetData(type, dateFrom, dateTo);

                return Ok(new ApiResponse
                {
                    RowCount = result.Count(),
                    TableData = result.ToDynamicList()
                });
            }
            else
            {
                var result = _cfr.GetDataCOA(type, coaCode, dateFrom, dateTo);

                return Ok(new ApiResponse
                {
                    RowCount = result.Count(),
                    TableData = result.ToDynamicList()
                });
            }
        }
    }
}
