using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("cash-bank-type")]
    [ApiController]
    public class CashBankTypeController : ControllerBase
    {
        private readonly ICashBankTypeService _type;

        public CashBankTypeController(ICashBankTypeService type)
        {
            _type = type;
        }

        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _type.GetLists()
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
