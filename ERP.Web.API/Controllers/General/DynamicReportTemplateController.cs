using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.General;

[Route("dynamic-report-template")]
[ApiController]
public class DynamicReportTemplateController : ControllerBase
{
    private readonly IDynamicReportTemplateService _service;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.DynamicReportTemplate;
    public DynamicReportTemplateController(IDynamicReportTemplateService service, IClaimService claim, IAuthService auth)
    {
        _service = service;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _service.GetData(
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

    [HttpGet("lists")]
    public IActionResult GetList(string sorts)
    {
        var data =
            _service.GetLists(
                    null,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                .ToDynamicList();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(DynamicReportTemplate data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _service.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, DynamicReportTemplate data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _service.Update(data);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public IActionResult OnDelete(int id)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        var result = _service.Delete(id, _claim.UserId);

        return Ok(result);
    }
}