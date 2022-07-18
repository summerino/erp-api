using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.SystemManagement;

[Route("[controller]")]
[ApiController]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menu;
    private readonly IClaimService _claim;

    public MenuController(IMenuService menu, IClaimService claim)
    {
        _menu = menu;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _menu.GetData(
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

    [HttpGet("navigation")]
    public IActionResult GetNavigation()
    {
        return Ok(_menu.GetNavigation(_claim.RoleId));
    }

    [HttpGet("hierarchy")]
    public IActionResult GetHierarchy()
    {
        return Ok(_menu.GetHierarchy());
    }

    [HttpGet("lists")]
    public IActionResult GetLists(int id)
    {
        var data = _menu.GetLists(id)
            .Select(x => new
            {
                x.Id,
                x.MenuId,
                x.ActionId,
                x.Seq
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }

    [HttpGet("menu/actions")]
    public IActionResult GetActions()
    {
        var data = _menu.GetActions()
            .Select(x => new
            {
                x.Id,
                x.Name,
                IsActive = false,
                IsChecked = false,
                Seq = 1
            })
            .ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = data.Count,
            TableData = data
        });
    }
}