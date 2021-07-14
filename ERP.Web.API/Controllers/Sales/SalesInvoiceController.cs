using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
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
    [Route("sales-invoice")]
    [ApiController]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ISalesInvoiceService _inv;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.SalesInvoice;
        public SalesInvoiceController(ISalesInvoiceService inv, IClaimService claim, IAuthService auth)
        {
            _inv = inv;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _inv.GetData(
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
        
        [HttpGet("detail")]
        public IActionResult GetDetailData(string code)
        {
            var data = _inv.GetDetailData(code)
                .Select(x => new
                {
                    x.Id, x.Code, x.LineNo, x.DoCode, x.ShipmentFee, x.HandlingFee,
                    x.SubTotal, x.FinalDisc, x.TaxAmount, x.Total, x.Dpp,
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
            var data = _inv.GetRelatedTransactions(code)
                .Select(x => new
                {
                    x.Code,
                    x.Date,
                    Total = x.Amount,
                    Type = "Kas Bank"
                }).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
        
        [HttpPost]
        public IActionResult OnPost(SalesInvoiceRequest data)
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

            var result = _inv.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, SalesInvoiceRequest data)
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

            var result = _inv.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _inv.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesInvoiceRequest data)
        {
            if (!data.Details.Any())
                return (false, "Item details can't be empty.");

            return data.Details.GroupBy(x => new { x.DoCode }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate receive code submitted.")
                : (true, "");
        }
    }
}
