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
using System;
using ERP.Web.API.Domain.Interfaces.Inventory;

namespace ERP.Web.API.Controllers.Mobile.Purchase
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class PurchaseOrderMobileController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrder;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;
        public PurchaseOrderMobileController(IPurchaseOrderService purchaseOrder, IUnitOfMeasurementService uom, IClaimService claim)
        {
            _purchaseOrder = purchaseOrder;
            _uom = uom;
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
        public IActionResult GetDetailData(string code, int srcTrans)
        {
            var data = _purchaseOrder.GetDetailDataForMobile(code, srcTrans);

            return Ok(data);
        }

        [HttpGet("log")]
        public IActionResult GetLogData(string filters, string sorts, string search, int skip, int take, string date)
        {
            DataSourceResult data =
                _purchaseOrder.GetLogDataForMobile(skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search, date);

            List<dynamic> result = ((List<ReceiveItemHeaderModel>)data.Data).ToList<dynamic>();

            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            }); ;
        }

        [HttpGet("logItem")]
        public IActionResult GetLogDetailData(string code, int srcTrans)
        {
            var data = _purchaseOrder.GetLogDetailDataForMobile(code, srcTrans);

            return Ok(data);
        }

        [HttpPost]
        public IActionResult OnPost(PurchaseOrderRequestModel data)
        {
            data.Mark = "A";
            data.ReceiveBy = _claim.UserId;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result =
                _purchaseOrder.InsertForMobile(data, _claim.UserId);

            return Ok(result);
        }

        [HttpGet("uomConversion")]
        public IActionResult GetUomConversionOne(int uomId)
        {
            var data = _uom.GetDataConversion(uomId);

            return Ok(data);
        }
    }
}
