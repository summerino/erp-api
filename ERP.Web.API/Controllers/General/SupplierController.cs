using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.General;

[Route("[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplier;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.Supplier;
    public SupplierController(ISupplierService supplier, IClaimService claim, IAuthService auth)
    {
        _supplier = supplier;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _supplier.GetData(
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
            _supplier.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                .ToDynamicList()
                .Select(x => new
                {
                    x.Code, x.Initial, x.Name, x.Address1, x.Phone, x.Fax
                })
                .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("{code}")]
    public IActionResult GetDataByCode(string code)
    {
        return Ok(_supplier.FindByCode(code));
    }

    [HttpPost]
    public IActionResult OnPost(Supplier data)
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

        var result = _supplier.Insert(data);

        return Ok(result);
    }

    [HttpPut("{code}")]
    public IActionResult OnPut(string code, Supplier data)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        data.UpdatedBy = _claim.UserId;
        data.UpdatedDate = DateTime.Now;

        var result = _supplier.Update(data);

        return Ok(result);
    }

    [HttpDelete("{code}")]
    public IActionResult OnDelete(string code)
    {
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
        {
            return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
        }
        var result = _supplier.Delete(code, _claim.UserId);

        return Ok(result);
    }
}