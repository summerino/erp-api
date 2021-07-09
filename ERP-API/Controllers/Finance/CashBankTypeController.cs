using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Domain.Interfaces.Finance;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ERP_API.Controllers.Finance
{
    [Route("general-cash-bank-type")]
    [ApiController]
    [AllowAnonymous]
    public class CashBankTypeController : ControllerBase
    {
        private readonly ICashBankTypeService _cashBankType;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        public CashBankTypeController(ICashBankTypeService cashBank, IClaimService claim, IAuthService auth)
        {
            _cashBankType = cashBank;
            _claim = claim;
            _auth = auth;
        }
        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _cashBankType.GetList().ToList<dynamic>();
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

    }
}
