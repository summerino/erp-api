using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileCustomer;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.CustomerOrder;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Mobile.CustomerTransaction;

[Authorize(AppConstant.ValidateMobileCustomerTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class CustomerOrderController:ControllerBase
{
    private readonly ICustomerOrderService _customerOrder;
    private readonly IClaimService _claim;

    public CustomerOrderController(ICustomerOrderService customerOrder, IClaimService claim)
    {
        _customerOrder = customerOrder;
        _claim = claim;
    }

    [HttpGet("tax")]
    public IActionResult GetTransactionTax(int taxId)
    {
        var data =
            _customerOrder.GetTransactionTax(taxId);
        return Ok(data);
    }

    [HttpGet("promotion-discount")]
    public IActionResult GetPromotionDiscount(int itemId, int itemCatId, decimal qty, int unitId)
    {
        var data =
            _customerOrder.GetPromotionDiscount(itemId, itemCatId, qty, unitId, _claim.UserCode);
        return Ok(data);
    }

    [HttpGet("promotion-freegood")]
    public IActionResult GetPromotionFreeGood(int itemId, int itemCatId, decimal qty, int unitId)
    {
        var data =
            _customerOrder.GetPromotionFreeGood(itemId, itemCatId, qty, unitId, _claim.UserCode);
        return Ok(data);
    }

    [HttpGet("promotion-invoice")]
    public IActionResult GetPromotionInvoice(decimal amount)
    {
        var data =
            _customerOrder.GetPromotionInvoice(amount, _claim.UserCode);
        return Ok(data);
    }

    [HttpGet("params")]
    public IActionResult GetParams()
    {
        var data =
            _customerOrder.GetParams();
        return Ok(data);
    }

    [HttpGet("order-header")]
    public IActionResult GetCustomerOrderHeader(string filters, string sorts, int skip, int take, DateTime? date)
    {
        var data =
            _customerOrder.GetCustomerOrderHeader(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), date, _claim.UserCode);


        var result = data.Data;

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }

    [HttpGet("order-detail")]
    public IActionResult GetCustomerOrderDetail(string orderCode)
    {
        var data =
            _customerOrder.GetCustomerOrderDetail(orderCode);
        return Ok(data);
    }

    [HttpPost("insert-order")]
    public IActionResult OnPost(OrderCustomerRequestModel data)
    {
        var header = data.Header;
        header.Mark = "A";
        header.Date = header.Date.Date;
        header.CreatedBy = _claim.UserId;
        header.CreatedDate = DateTime.Now;
        header.UpdatedBy = header.CreatedBy;
        header.UpdatedDate = header.CreatedDate;

        var result =
            _customerOrder.InsertOrderCustomer(header,data.Details);

        return Ok(result);
    }
}