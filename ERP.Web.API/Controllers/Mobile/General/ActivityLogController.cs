using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.General;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.General;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService _activityLog;
    private readonly IClaimService _claim;

    public ActivityLogController(IActivityLogService activityLog, IClaimService claim)
    {
        _activityLog = activityLog;
        _claim = claim;
    }

    [HttpPost]
    public IActionResult OnPost(IEnumerable<LogActivityRequestModel> data)
    {
        var result = _activityLog.Insert(data, _claim.UserId);

        return Ok(result);
    }
}