using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Mobile.CustomerTransaction;

[Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class CustomerTransactionController : ControllerBase
{
    private readonly ICustomerTransactionService _customerTransaction;
    private readonly IClaimService _claim;

    public CustomerTransactionController(ICustomerTransactionService customerTransaction, IClaimService claim)
    {
        _customerTransaction = customerTransaction;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string filters, string sorts, int skip, int take, DateTime? date)
    {
           
        var data =
            _customerTransaction.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), date, _claim.UserCode);


        var result = data.Data.ToDynamicList().Select(x => new
        {
            x.Code,
            x.Date,
            x.FinalDiscPercent,
            x.FinalDisc,
            x.SubTotal,
            x.TaxAmount,
            x.Total,
            Status = x.Remaining == 0 ? "Lunas" : "Belum"
        }).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }

    [HttpGet("items")]
    public IActionResult GetTransactionItem(string code)
    {
        var data =
            _customerTransaction.GetTransactionItem(code);
        return Ok(data);
    }

    [HttpGet("credit-limit")]
    public IActionResult GetCreditLimit()
    {
        var data =
            _customerTransaction.GetCreditLimit(_claim.UserCode);
        return Ok(data);
    }

    [HttpGet("profile")]
    public IActionResult GetCustomerProfile()
    {
        var data =
            _customerTransaction.GetCustomerProfile(_claim.UserCode);
        return Ok(data);
    }
}