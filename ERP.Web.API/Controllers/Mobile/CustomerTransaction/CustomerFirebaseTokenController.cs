using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.General
{
    [Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class CustomerFirebaseTokenController : ControllerBase
    {
        private readonly ICustomerFirebaseTokenService _token;
        private readonly IClaimService _claim;

        public CustomerFirebaseTokenController(ICustomerFirebaseTokenService token, IClaimService claim)
        {
            _token = token;
            _claim = claim;
        }

        [HttpPost]
        public IActionResult OnPost(string firebaseTokenId)
        {
            var result = _token.AddCustomerFirebaseToken(firebaseTokenId, _claim.UserCode);
            return Ok(result);
        }
    }
}
