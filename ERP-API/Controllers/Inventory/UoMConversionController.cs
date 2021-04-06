using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Dtos;

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
        public IActionResult GetData([FromQuery] int uomId) 
        {
            var data =
                _uomC.GetData(uomId)
                    .Select(x => new
                    {
                        x.Id, x.UomId, x.UnitToConvert, x.UnitEquivalent, x.Conversion,
                        x.IsBaseUnit, x.Seq
                    })
                    .ToList<dynamic>();

            return Ok(new MasterViewDto
            {
                RowCount = data.Count,
                TableData = data
            });
        }
    }
}
