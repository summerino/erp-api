using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.General;

[Route("[controller]")]
[ApiController]
public class TaxController : ControllerBase
{
    private readonly ITaxService _tax;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;

    private const int MenuId = (int)Menu.Tax;

    public TaxController(ITaxService tax, IClaimService claim, IAuthService auth)
    {
        _tax = tax;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _tax.GetData(
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
    public IActionResult GetList(string filters, string sorts) 
    {
        var data =
            _tax.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                .ToDynamicList()
                .Select(x => new
                {
                    x.Id, x.Initial, x.Name, x.Rate
                })
                .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpPost]
    public IActionResult OnPost(Tax data)
    {

        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.IsActive = true;
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _tax.Insert(data);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public IActionResult OnPut(string id, Tax data)
    {

        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _tax.Update(data);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public IActionResult OnDelete(int id)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }

        var result = _tax.Delete(id, _claim.UserId);

        return Ok(result);
    }
}