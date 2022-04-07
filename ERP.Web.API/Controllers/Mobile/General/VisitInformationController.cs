using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Mobile.General;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class VisitInformationController : ControllerBase
{
    private readonly IVisitInformationService _visitInformation;
    private readonly IClaimService _claim;

    public VisitInformationController(IVisitInformationService visitInformation, IClaimService claim)
    {
        _visitInformation = visitInformation;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetTotalVisit(int year, int month)
    {
        return Ok(_visitInformation.GetVisitInformation(
            year, month, _claim.UserId));
    }
}