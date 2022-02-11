using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model;
using ERP.Web.API.Model.General;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.General;

[Route("active-transaction")]
[ApiController]
public class ActiveTransactionController : ControllerBase
{
    private readonly IActiveTransactionService _activeTrans;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;

    private const int MenuId = (int)Menu.ActiveTransaction;

    public ActiveTransactionController(IActiveTransactionService activeTrans, IClaimService claim, IAuthService auth)
    {
        _activeTrans = activeTrans;
        _claim = claim;
        _auth = auth;
    }

    [HttpPut("[action]")]
    public IActionResult Locked(ActiveTransactionRequest data)
    {
        var result = _activeTrans.LockedTransaction(data, _claim.UserId);

        return Ok(result);
    }

    [HttpPut("[action]")]
    public IActionResult Released(ActiveTransactionRequest data)
    {
        var result = _activeTrans.ReleasedTransaction(data, _claim.UserId);

        return Ok(result);
    }
}