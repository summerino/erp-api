using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileSales
{
    [Route("mobile-map-tracking-report")]
    [ApiController]
    public class MobileMapTrackingReportController : ControllerBase
    {
        private readonly IMobileMapTrackingReportService _mmt;
        public MobileMapTrackingReportController(IMobileMapTrackingReportService mmt)
        {
            _mmt = mmt;
        }

        [HttpGet]
        public IActionResult GetData(int salesId, int type, string startDate, string endDate)
        {
            var result = _mmt.GetData(salesId, type, startDate, endDate);

            var data = result.Select(x => new
            {
                x.Lat,
                x.Lng
            }).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = result.Count(),
                TableData = data
            });
        }
    }
}
