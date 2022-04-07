
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.Warehouse;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.Warehouse;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class WarehouseProfileController : ControllerBase
{
    private readonly IWarehouseProfileService _warehouseProfile;
    private readonly IClaimService _claim;

    public WarehouseProfileController(IWarehouseProfileService warehouseProfile, IClaimService claim)
    {
        _warehouseProfile = warehouseProfile;
        _claim = claim;
    }

    [HttpGet("profile")]
    public IActionResult GetSalesProfile()
    {
        return Ok(_warehouseProfile.GetWarehouseProfileForMobile(_claim.UserId));
    }
}