using System;
using System.Linq;
using ERP.Common;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using ERP.Web.API.Domain.Interfaces.Accounting;
using System.Collections.Generic;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("direct-invoice")]
    [ApiController]
    public class DirectInvoiceController : ControllerBase
    {
        private readonly IDirectInvoiceService _inv;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IClosingMonthService _closingMonth;
        private const int _menuId = (int)Menu.DirectInvoice;

        public DirectInvoiceController(IDirectInvoiceService inv, ISystemParameterService sysPar,
            IClaimService claim, IAuthService auth, IClosingMonthService closingMonthService)
        {
            _inv = inv;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
            _closingMonth = closingMonthService;
        }
        
        [HttpGet("{code}")]
        public IActionResult GetDataByCode(string code)
        {
            return Ok(_inv.FindByCode(code));
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
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

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
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

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

        [HttpDelete]
        public IActionResult OnDelete(SalesInvoiceRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _inv.Delete(data.Code, _claim.UserId);

            return Ok(result);
        }

        private (bool, string) Validate(SalesInvoiceRequest data)
        {
            var periods = new List<string> { data.Date.ToString("yyyyMM"), data.DueDate.ToString("yyyyMM") };
            if (data.OriginalDate.HasValue)
                periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));
            if (data.OriginalDueDate.HasValue)
                periods.Add(data.OriginalDueDate.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.Date))
                return (false, "Tanggal Transaksi tidak boleh lebih kecil dari tanggal mulai data.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.DueDate))
                return (false, "Tanggal Jatuh Tempo tidak boleh lebih kecil dari tanggal mulai data.");

            if (data.ItemDetails != null)
            {
                if (!data.ItemDetails.Any())
                    return (false, "Detail tidak boleh kosong.");

                if (data.ItemDetails.GroupBy(x => new { x.ItemId }).Any(x => x.Count() > 1))
                    return(false, "Terdapat kode pengiriman yang sama pada bagian detail.");
            }

            return (true, "");
        }
    }
}
