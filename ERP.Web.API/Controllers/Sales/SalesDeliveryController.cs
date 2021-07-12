using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("sales-delivery")]
    [ApiController]
    public class SalesDeliveryController : ControllerBase
    {
        private readonly ISalesDeliveryService _dlv;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.SalesDelivery;
        public SalesDeliveryController(ISalesDeliveryService dlv, IClaimService claim, IAuthService auth)
        {
            _dlv = dlv;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _dlv.GetData(
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

        [HttpGet("item")]
        public IActionResult GetDetailData(string code)
        {
            var data = _dlv.GetDetailData(code)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo, x.SoDetailId, x.ItemId, x.ItemInitial, x.ItemName,
                    x.OrderQty, x.OutstandingQty, x.Qty,
                    x.UomId, x.UnitId, x.UnitName,
                    x.Length, x.Width, x.Height, x.Weight, x.DimensionMeasurement, x.WeightMeasurement,
                    x.UnitPrice, x.Disc, x.TaxId, x.TaxAmount, x.NettPrice, x.Total, x.Dpp,
                    OldUnitId = x.ItemUomSellId,
                    OldUnitName = x.ItemUomSellName,
                    OldUnitPrice = x.ItemSellPrice,
                    TotTax = x.Qty * x.TaxAmount,
                    TotDPP = x.Qty * x.Dpp,
                    State = ""
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("related-trans")]
        public IActionResult GetRelatedTransactions(string code)
        {
            var data = _dlv.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("un-invoice")]
        public IActionResult GetUnInvoiceData(string soCode, string invCode)
        {
            var data = _dlv.GetUnInvoiceData(soCode, invCode).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(SalesDeliveryRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Insert process
            data.Mark = "A";
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _dlv.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, SalesDeliveryRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _dlv.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _dlv.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesDeliveryRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
                return (false, "There are duplicate item submitted with same unit.");
            
            return data.ItemDetails.Sum(x => x.Qty) <= 0
                ? (false, "Total delivery qty can't be 0.")
                : (true, "");
        }
    }
}
