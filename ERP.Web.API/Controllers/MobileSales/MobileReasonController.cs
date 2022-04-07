using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.MobileSales;

[Route("mobile-reason")]
[ApiController]
public class MobileReasonController : ControllerBase
{
    private readonly IMobileReasonService _mr;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;

    private const int MenuId = (int)Menu.MobileReason;

    public MobileReasonController(IMobileReasonService mr, IClaimService claim, IAuthService auth)
    {
        _mr = mr;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _mr.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search);

        return Ok(new ApiResponse
        {
            RowCount = data.Total,
            TableData = data.Data.ToDynamicList()
        });
    }

    [HttpPost]
    public IActionResult OnPost(MobileReason data)
    {

        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _mr.Insert(data);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public IActionResult OnPut(string id, MobileReason data)
    {

        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _mr.Update(data);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public IActionResult OnDelete(int id)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        var result = _mr.Delete(id, _claim.UserId);
        return Ok(result);
    }
}