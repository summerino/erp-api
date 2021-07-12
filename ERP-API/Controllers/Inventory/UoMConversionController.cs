using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("uom-conversion")]
    [ApiController]
    public class UoMConversionController : ControllerBase
    {
        private readonly IUnitOfMeasurementService _uom;

        public UoMConversionController(IUnitOfMeasurementService uom)
        {
            _uom = uom;
        }

        [HttpGet]
        public IActionResult GetData(int uomId)
        {
            var data =
                _uom.GetDataConversion(uomId)
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
