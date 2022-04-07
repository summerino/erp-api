using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("salesman-target")]
[ApiController]
public class SalesTargetController : ControllerBase
{
    private readonly ISalesTargetService _salesTarget;
    private readonly ISystemParameterService _sysPar;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.SalesmanTarget;

    public SalesTargetController(ISalesTargetService salesTarget, ISystemParameterService sysPar, IClaimService claim, IAuthService auth)
    {
        _salesTarget = salesTarget;
        _sysPar = sysPar;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _salesTarget.GetData(
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

    [HttpGet("detail")]
    public IActionResult GetDetailData(string code)
    {
        var data = _salesTarget.GetDetailData(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("subject")]
    public IActionResult GetSubjectData(string code)
    {
        var data = _salesTarget.GetSubjectData(code).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(SalesTargetRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Insert process
        data.Mark = "A";
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _salesTarget.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(SalesTargetRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        // Update process
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _salesTarget.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code, SalesTargetRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        // Validate process
        var (isValid, message) = Validate(data, true);
        if (!isValid)
            return Ok(new SaveResult(false, message));

        var result = _salesTarget.Delete(data.Code, _claim.UserId);

        return Ok(result);
    }

    [HttpPost("clone")]
    public IActionResult OnClone(SalesTargetRequest data)
    {
        // Checking role authorization
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Clone }).Any())
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

        var result = _salesTarget.Clone(data.Code, data.Name, data.StartDate, data.EndDate, _claim.UserId);

        return Ok(result);
    }

    private (bool, string) Validate(SalesTargetRequest data, bool onDelete = false)
    {
        // Checking data start date validity
        if (!_sysPar.IsStartDateValid(data.StartDate))
            return (false, "Tanggal Mulai tidak boleh lebih kecil dari tanggal mulai data.");

        // Checking data start date validity
        if (!_sysPar.IsStartDateValid(data.EndDate))
            return (false, "Tanggal Akhir tidak boleh lebih kecil dari tanggal mulai data.");

        if (!onDelete)
        {
            if (!data.ItemDetails.Any())
                return (false, "Detail tidak boleh kosong.");

            if (!data.ItemSubjects.Any())
                return (false, "Subyek tidak boleh kosong.");

            if (data.ItemDetails.GroupBy(x => new { x.Code, x.ItemGroupId, x.ItemSubGroupId, x.SubGroup }).Any(x => x.Count() > 1))
                return (false, "Terdapat data detail yang sama.");

            if (data.ItemSubjects.GroupBy(x => new { x.Code, x.SalesmanId }).Any(x => x.Count() > 1))
                return (false, "Terdapat data subyek yang sama.");
        }

        return (true, "");
    }
}