using Microsoft.AspNetCore.Mvc;
using ERP.Common.Extensions;
using ERP.Web.API.Domain.Interfaces.SystemManagement;

namespace ERP.Web.API.Controllers.SystemManagement;

[Route("[controller]")]
[ApiController]
public class ActionController : ControllerBase
{
    private readonly IActionService _action;

    public ActionController(IActionService action)
    {
        _action = action;
    }

    [HttpGet]
    public IActionResult GetData()
    {
        var data =
            _action.GetData()
                .Select(x => new
                {
                    x.Id, x.Name,
                    Initial = x.Name.RemoveSpecialCharacter()
                    //Initial = Regex.Replace(x.Name, @"[^0-9a-zA-Z]+", "")
                });

        return Ok(data);
    }
}