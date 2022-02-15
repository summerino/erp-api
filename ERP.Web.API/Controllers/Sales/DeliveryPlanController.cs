using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("delivery-plan")]
    [ApiController]
    public class DeliveryPlanController : ControllerBase
    {
        private readonly IDeliveryPlanService _dp;
        private readonly ISystemParameterService _sysPar;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IClosingMonthService _closingMonth;
        private readonly IActiveTransactionService _activeTrans;
        private const int MenuId = (int)Menu.DeliveryPlan;

        public DeliveryPlanController(IDeliveryPlanService deliveryPlan, ISystemParameterService sysPar, 
            IClaimService claim, IAuthService auth, IClosingMonthService closingMonthService, IActiveTransactionService activeTrans)
        {
            _dp = deliveryPlan;
            _sysPar = sysPar;
            _claim = claim;
            _auth = auth;
            _closingMonth = closingMonthService;
            _activeTrans = activeTrans;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _dp.GetData(
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
            var uData = _dp.GetUndeliveredData();
            var data = _dp.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    x.TransCode,
                    x.Volume,
                    x.Weight,
                    x.SrcTrans,
                    x.IsFailShipment,
                    x.NotesFailShipment,
                    x.FailedSendAll,
                    UndeliveredItems = uData.Where(d => d.DlvPlanDetailId == x.Id).OrderBy(d => d.LineNo)
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
            var data = _dp.GetRelatedTransactions(code);

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("all-trans")]
        public IActionResult GetAllTransaction(string warehouseCode, string filters)
        {
            var data = _dp.GetAllTransaction(warehouseCode,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpPost]
        public IActionResult OnPost(DeliveryPlanRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data, checkSeenByOther: false);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Insert process
            data.Mark = "A";
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _dp.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, DeliveryPlanRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _dp.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code, DeliveryPlanRequest data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Void }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Validate process
            var (isValid, message) = Validate(data, true);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            var result = _dp.Delete(data.Code, _claim.UserId);

            return Ok(result);
        }

        private (bool, string) Validate(DeliveryPlanRequest data, bool onDelete = false, bool checkSeenByOther = true)
        {
            var periods = new List<string> { data.Date.ToString("yyyyMM") };
            if (data.OriginalDate.HasValue)
                periods.Add(data.OriginalDate.Value.ToString("yyyyMM"));

            if (_closingMonth.IsMonthClosed(periods))
                return (false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi.");

            // Checking data start date validity
            if (!_sysPar.IsStartDateValid(data.Date))
                return (false, "Tanggal tidak boleh lebih kecil dari tanggal mulai data.");

            if (!onDelete)
            {
                if (!data.ItemDetails.Any())
                    return (false, "Detail tidak boleh kosong.");

                if (data.ItemDetails.GroupBy(x => new { x.Code, x.TransCode }).Any(x => x.Count() > 1))
                    return(false, "Terdapat transaksi yang sama pada bagian detail.");
            }

            // Checking is data seen by others
            if (checkSeenByOther && !_activeTrans.SeenByOthers("DP", data.Code, _claim.UserId))
            {
                return (false, "data sedang digunakan oleh pengguna lain.");
            }

            return (true, "");
        }
    }
}
