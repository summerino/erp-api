using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Domain.Models.Mobile.Purchase;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.Purchase
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrder;
        private readonly IClaimService _claim;
        public PurchaseOrderController(IPurchaseOrderService purchaseOrder, IClaimService claim)
        {
            _purchaseOrder = purchaseOrder;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string filters, string sorts, string search, int skip, int take, string date)
        {
            var data =
                _purchaseOrder.GetDataForMobile(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search, date);

            var result = ((List<PurchaseOrderHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            }); ;
        }

        [HttpGet("item")]
        public IActionResult GetDetailData(string code, bool? fullReceived)
        {
            //var uomC = _uom.GetDataConversion().ToList();

            var data = _purchaseOrder.GetDetailDataForMobile(code, fullReceived)
                .Select(x => new
                {
                    x.Code,
                    x.ItemId,
                    x.ItemInitial,
                    x.ItemName,
                    x.LineNo,
                    x.OrderQty,
                    x.ReceiveQty,
                    x.RemainQty, // kuantitas sisa?
                    x.Uom, // ambil description?
                    x.UomId,
                    //Units = uomC.Where(u => u.UomId == x.UomId)
                    //    .Select(u => new
                    //    {
                    //        u.Id,
                    //        u.UomId,
                    //        u.UnitToConvert,
                    //        u.UnitEquivalent,
                    //        u.Conversion,
                    //        u.IsBaseUnit,
                    //        u.Seq
                    //    })
                    //    .OrderBy(u => u.Seq)
                    //    .ToList(),
                    //OldUnitId = x.ItemUomBuyId,
                    //OldUnitName = x.ItemUomBuyName,
                    //OldUnitPrice = x.ItemBuyPrice,
                    //TotTax = x.Qty * x.TaxAmount,
                    //TotDPP = x.Qty * x.Dpp,
                    //State = ""
                }).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Count,
                Data = data
            });
        }
    }
}
