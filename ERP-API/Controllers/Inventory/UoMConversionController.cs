using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Model;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/uom-conversion")]
    //[Authorize]
    [ApiController]
    public class UoMConversionController : ControllerBase
    {
        private readonly IUoMConversionService _uomC;

        public UoMConversionController(IUoMConversionService uomC)
        {
            _uomC = uomC;
        }

        [HttpGet]
        public IActionResult GetData(int uomId)
        {
            var data =
                _uomC.GetData(uomId)
                    .Select(x => new
                    {
                        x.Id, x.UomId, x.UnitToConvert, x.UnitEquivalent, x.Conversion,
                        x.IsBaseUnit, x.Seq
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
    }
}
