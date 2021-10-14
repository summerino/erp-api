using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.Sales
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class SalesmanController:ControllerBase
    {
        private readonly ISalesmanService _salesman;
        private readonly IClaimService _claim;

        public SalesmanController(ISalesmanService salesman, IClaimService claim)
        {
            _salesman = salesman;
            _claim = claim;
        }

        [HttpGet("profile")]
        public IActionResult GetSalesProfile()
        {
            return Ok(_salesman.GetSalesProfileForMobile(_claim.UserId));
        }
    }
}
