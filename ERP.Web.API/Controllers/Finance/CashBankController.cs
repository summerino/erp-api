using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Finance;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("general-cash-bank")]
    [ApiController]
    [AllowAnonymous]
    public class CashBankController : ControllerBase
    {
        private readonly ICashBankService _cashBank;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.CashBank;
        public CashBankController(ICashBankService cashBank, IClaimService claim, IAuthService auth)
        {
            _cashBank = cashBank;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cashBank.GetData(
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
            var data = _cashBank.GetDetailData(code).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("ap")]
        public IActionResult GetDataAP(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cashBank.GetDataAP(
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

        [HttpGet("ar")]
        public IActionResult GetDataAR(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cashBank.GetDataAR(
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
        [HttpGet("debit-memo")]
        public IActionResult GetDataDebitMemo(string cashbankCode, string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cashBank.GetDataDebitMemo(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    search, cashbankCode);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("credit-memo")]
        public IActionResult GetDataCreditMemo(string cashbankCode, string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cashBank.GetDataCreditMemo(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    search, cashbankCode);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpPost]
        public IActionResult OnPost(CashBankRequest data)
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

            var result = _cashBank.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, CashBankRequest data)
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

            var result = _cashBank.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            var result = _cashBank.Delete(code, _claim.UserId);

            return Ok(result);
        }
        private static (bool, string) Validate(CashBankRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Data detil tidak boleh kosongs.");

            return (true, "");
        }
    }
}
