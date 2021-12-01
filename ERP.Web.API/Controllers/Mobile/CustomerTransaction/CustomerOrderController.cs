using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Mobile.CustomerTransaction
{
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

    }
}
