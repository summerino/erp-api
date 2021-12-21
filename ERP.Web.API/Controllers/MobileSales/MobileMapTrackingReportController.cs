using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.MobileSales;

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
        public IActionResult GetData(string date, int salesId, int type)
        {
            var mapTrackingData = _mmt.GetData(date, salesId, type);
            var customerData = _mmt.GetCustomerData(date, salesId);
            
            return Ok(new
            {
                Tracking = mapTrackingData.Select(x => new
                {
                    x.TrackedDate,
                    x.Lat,
                    x.Lng
                }),
                Customer = customerData
            });
        }
    }
}
