using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Purchase;
using Newtonsoft.Json;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Auth;

namespace ERP_API.Controllers.Purchase
{
    [Route("api/v1/purchase-return")]
    [ApiController]
    public class PurchaseReturnController : ControllerBase
    {
        private readonly IPurchaseReturnService _rtn;
        private readonly IClaimService _claim;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.PurchaseReturn;

        public PurchaseReturnController(IPurchaseReturnService rtn, IClaimService claim, IUnitOfMeasurementService uom, IAuthService auth)
        {
            _rtn = rtn;
            _claim = claim;
            _uom = uom;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _rtn.GetData(
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
        public IActionResult GetDetailData(string code, bool? fullReceived)
        {
            var uomC = _uom.GetDataConversion().ToList();
            var data = _rtn.GetDetailData(code, fullReceived)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.RcvDetailId,
                    x.ItemId,
                    x.ItemName,
                    x.UomId,
                    x.UnitId,
                    x.UnitName,
                    x.Qty,
                    x.QtyRcv,
                    x.WarehouseCode,
                    x.WarehouseCodeIn,
                    x.Length,
                    x.Width,
                    x.Height,
                    x.Weight,
                    x.DimensionMeasurement,
                    x.WeightMeasurement,
                    x.UnitPrice,
                    x.Disc,
                    x.TaxId,
                    x.TaxAmount,
                    x.NettPrice,
                    x.Total,
                    x.Dpp,
                    OldUnitId = x.ItemUomBuyId,
                    OldUnitName = x.ItemUomBuyName,
                    OldUnitPrice = x.ItemBuyPrice,
                    Units = uomC.Where(u => u.UomId == x.UomId)
                        .Select(u => new
                        {
                            u.Id,
                            u.UomId,
                            u.UnitToConvert,
                            u.UnitEquivalent,
                            u.Conversion,
                            u.IsBaseUnit,
                            u.Seq
                        })
                        .OrderBy(u => u.Seq)
                        .ToList(),
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

        [HttpGet("diff-item")]
        public IActionResult GetDiffItem(string code)
        {
            var uomC = _uom.GetDataConversion().ToList();

            var data = _rtn.GetDetailExchangeData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.ReturnDetailId,
                    x.ItemId,
                    x.ItemInitial,
                    x.ItemName,
                    x.UomId,
                    x.UnitId,
                    x.UnitName,
                    x.Qty,
                    x.UnitPrice,
                    x.TaxId,
                    x.TaxAmount,
                    x.NettPrice,
                    x.Total,
                    x.Dpp,
                    OldUnitId = x.ItemUomBuyId,
                    OldUnitName = x.ItemUomBuyName,
                    OldUnitPrice = x.ItemBuyPrice,
                    TotTax = x.Qty * x.TaxAmount,
                    TotDPP = x.Qty * x.Dpp,
                    Units = uomC.Where(u => u.UomId == x.UomId)
                        .Select(u => new
                        {
                            u.Id,
                            u.UomId,
                            u.UnitToConvert,
                            u.UnitEquivalent,
                            u.Conversion,
                            u.IsBaseUnit,
                            u.Seq
                        })
                        .OrderBy(u => u.Seq)
                        .ToList(),
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
            var data = _rtn.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
        
        [HttpPost]
        public IActionResult OnPost(PurchaseReturnRequest data)
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

            var result = _rtn.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, PurchaseReturnRequest data)
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

            var result = _rtn.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _rtn.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(PurchaseReturnRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
                return (false, "There are duplicate item submitted with same unit.");

            return data.ItemDetails.Sum(x => x.Qty) <= 0
                ? (false, "Total receive qty can't be 0.")
                : (true, "");
        }
    }
}
