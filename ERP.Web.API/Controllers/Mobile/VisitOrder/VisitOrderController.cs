using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Mobile.VisitOrder;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Mobile.VisitOrder;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class VisitOrderController : ControllerBase
{
    private readonly IVisitOrderService _visitOrder;
    private readonly IClaimService _claim;

    public VisitOrderController(IVisitOrderService visitOrder, IClaimService claim)
    {
        _visitOrder = visitOrder;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string lastUpdate)
    {
        var data =
            _visitOrder.GetData(_claim.UserId, lastUpdate);

        var temp = ((List<VwVisitOrder>)data.Data);

        var result = temp.Select(x => new
        {
            x.Code,
            x.Date,
            x.Status,
            x.Notes,
            x.UpdatedDate
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("detail")]
    public IActionResult GetDetail(string code)
    {
        var data =
            _visitOrder.GetDetail(code);

        return Ok(data);
    }

    [HttpGet("reason")]
    public IActionResult GetReason(string lastUpdate)
    {
        var data =
            _visitOrder.GetReason(lastUpdate).Select(x => new
            {
                x.Id,
                x.Type,
                x.Name,
                x.IsActive,
                x.UpdatedDate
            });

        return Ok(data);
    }

    [HttpGet("customer")]
    public IActionResult GetVisitOrderCustomer(string lastUpdate)
    {
        var data =
            _visitOrder.GetVisitOrderCustomer(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("log")]
    public IActionResult GetVisitLog(string lastUpdate)
    {
        var data =
            _visitOrder.GetLogVisit(_claim.UserId, lastUpdate).Select(x => new
            {
                x.Code,
                x.Date,
                x.VisitOrderCode,
                x.CustCode,
                x.Scheduled,
                x.Visited,
                x.Lat,
                x.Lng,
                x.StartTime,
                x.EndTime,
                x.Total,
                x.UnscheduledVisitReasonId,
                x.NoVisitReasonId,
                x.NoOrderReasonId,
                x.Image,
                x.UpdatedDate
            });

        return Ok(data);
    }

    [HttpGet("visitReason")]
    public IActionResult GetVisitReason(string lastUpdate)
    {
        var data =
            _visitOrder.GetVisitReason(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("invoice")]
    public IActionResult GetVisitOrderInvoice(string lastUpdate)
    {
        var data =
            _visitOrder.GetVisitOrderInvoice(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("salesInvoice")]
    public IActionResult GetSalesInvoiceHeader(string lastUpdate)
    {
        var data =
            _visitOrder.GetSalesInvoceHeader(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("payment")]
    public IActionResult GetPaymentInvoice(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobilePaymentInvoice(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("orderHeader")]
    public IActionResult GetOrderHeader(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobileOrderHeaders(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("orderDetail")]
    public IActionResult GetOrderDetail(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobileOrderDetails(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("orderDetailDiscount")]
    public IActionResult GetOrderDetailDiscount(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobileOrderDetailDiscounts(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("orderDetailFreeGoods")]
    public IActionResult GetOrderDetailFreeGoods(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobileOrderDetailFreeGoods(_claim.UserId, lastUpdate);

        return Ok(data);
    }

    [HttpGet("promoHeader")]
    public IActionResult GetPromoHeader(string lastUpdate)
    {
        var data =
            _visitOrder.GetPromoHeader(lastUpdate).Select(x => new
            {
                x.Code,
                x.Name,
                x.StartDate,
                x.EndDate,
                x.ApplyTo,
                x.CoaCost,
                x.Mark,
                x.UpdatedDate
            }); ;

        return Ok(data);
    }

    [HttpGet("promoDetail")]
    public IActionResult GetPromoDetail(string lastUpdate)
    {
        var data =
            _visitOrder.GetPromoDetail(lastUpdate);

        return Ok(data);
    }

    [HttpGet("promoDetailMultipleItem")]
    public IActionResult GetPromoDetailMultipleItem(string lastUpdate)
    {
        var data =
            _visitOrder.GetPromoDetailMultipleItem(lastUpdate);

        return Ok(data);
    }

    [HttpGet("promoDetailTier")]
    public IActionResult GetPromoDetailTier(string lastUpdate)
    {
        var data =
            _visitOrder.GetPromoDetailTier(lastUpdate);

        return Ok(data);
    }

    [HttpGet("promoSubject")]
    public IActionResult GetPromoSubject(string lastUpdate)
    {
        var data =
            _visitOrder.GetPromoSubjects(lastUpdate);

        return Ok(data);
    }

    [HttpGet("tax")]
    public IActionResult GetTaxes(string lastUpdate)
    {
        var data =
            _visitOrder.GetTax(lastUpdate).Select(x => new
            {
                x.Id,
                x.Initial,
                x.Name,
                x.TypeId,
                x.Rate,
                x.ExemptRate,
                x.CoaCode,
                x.ExemptCoaCode,
                x.Seq,
                x.IsActive,
                x.UpdatedDate
            });

        return Ok(data);
    }

    [HttpGet("paymentTerm")]
    public IActionResult GetPaymentTerms(string lastUpdate)
    {
        var data =
            _visitOrder.GetPaymentTerms(lastUpdate).Select(x => new
            {
                x.Id,
                x.Initial,
                x.Name,
                x.Due,
                x.IsActive,
                x.UpdatedDate
            });

        return Ok(data);
    }

    [HttpPost]
    public IActionResult OnPost(VisitRequestModel data)
    {
        data.Mark = "A";
        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result =
            _visitOrder.Insert(data);

        return Ok(result);
    }

    [HttpGet("paymentMethod")]
    public IActionResult GetPaymentMethod(string lastUpdate)
    {
        var data =
            _visitOrder.GetMobilePaymentMethod(lastUpdate).Select(x => new
            {
                x.Id,
                x.Name,
                x.CoaCode,
                x.Seq,
                x.IsActive,
                x.UpdatedDate
            });

        return Ok(data);
    }

    [HttpGet("logDate")]
    public IActionResult GetListDate(DateTime date)
    {
        var data =
            _visitOrder.GetListDate(_claim.UserId, date);

        return Ok(data);
    }

    [HttpGet("logByDate")]
    public IActionResult GetVisitLogByDate(DateTime date)
    {
        var data =
            _visitOrder.GetVisitLogByDate(_claim.UserId, date);

        return Ok(data);
    }

    [HttpGet("detailVisitLog")]
    public IActionResult GetDetailVisitLog(string code)
    {
        var data =
            _visitOrder.GetDetailVisitLog(code);
        return Ok(data);
    }

    [HttpGet("orderDetailRequest")]
    public IActionResult GetOrderDetailRequest(string code)
    {
        var data =
            _visitOrder.GetOrderDetailRequest(code);
        return Ok(data);
    }

    [HttpGet("orderHeaderByCode")]
    public IActionResult GetOrderHeaderByCode(string code)
    {
        var data =
            _visitOrder.GetOrderHeader(code);
        return Ok(data);
    }

    [HttpGet("itemCategory")]
    public IActionResult GetItemCategories(string lastUpdate)
    {
        var data =
            _visitOrder.GetItemCategories(lastUpdate).Select(x => new
            {
                x.Id,
                x.Initial,
                x.Name,
                x.ParentId,
                x.GroupId,
                x.Deep,
                x.Seq,
                x.Lineage,
                x.IsActive,
                x.UpdatedDate
            });

        return Ok(data);
    }
}