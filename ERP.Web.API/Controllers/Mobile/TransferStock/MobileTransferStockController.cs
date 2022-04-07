using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.TransferStock;
using ERP.Web.API.Domain.Models.Mobile.TransferStock;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.TransferStock;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class MobileTransferStockController : ControllerBase
{

    private readonly IMobileTransferStockService _transferStock;
    private readonly IClaimService _claim;
    public MobileTransferStockController(IMobileTransferStockService transferStock, IClaimService claim)
    {
        _transferStock = transferStock;
        _claim = claim;
    }

    [HttpGet("header")]
    public IActionResult GetTransferStockHeader(DateTime? date, string search)
    {
        var result = _transferStock.getTranferStockHeader(date, search, _claim.UserId);

        return Ok(result);
    }

    [HttpGet("detail")]
    public IActionResult GetTransferStockDetail(string code)
    {
        var result = _transferStock.getTransferStockDetail(code);

        return Ok(result);
    }

    [HttpGet("log-header")]
    public IActionResult GetMobileTransferStockHeader(DateTime? date, string search)
    {
        var result = _transferStock.getMobileTranferStockHeader(date, search, _claim.UserId);

        return Ok(result);
    }

    [HttpGet("log-detail")]
    public IActionResult GetMobileTransferStockDetail(string code)
    {
        var result = _transferStock.getMobileTransferStockDetail(code);

        return Ok(result);
    }

    [HttpPost]
    public IActionResult OnPost(TransferStockRequestModel data)
    {

        var result = _transferStock.Insert(data, _claim.UserId);

        return Ok(result);
    }
}