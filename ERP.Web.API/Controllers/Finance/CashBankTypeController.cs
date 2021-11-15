using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model;
using ERP.Web.API.Domain.Interfaces.Auth;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("cash-bank-type")]
    [ApiController]
    public class CashBankTypeController : ControllerBase
    {
        private readonly ICashBankTypeService _type;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int MenuId = (int)Menu.CashBank;

        public CashBankTypeController(ICashBankTypeService type, IClaimService claim, IAuthService auth)
        {
            _type = type;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _type.GetLists(_auth.GetActions(MenuId, _claim.RoleId, new List<int>()).ToList())
                            .Select(x => new
                            {
                                x.Code, x.Name,
                                x.CoaCode, x.CoaName
                            }).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
    }
}
