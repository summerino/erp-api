using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Accounting;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("closing-month")]
    [ApiController]

    public class ClosingMonthController : ControllerBase
    {
        private readonly IClosingMonthService _cm;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IApprovalService _apv;
        private const int _menuId = (int)Menu.ClosingMonth;

        public ClosingMonthController(IClosingMonthService closingMonth, IClaimService claim, IAuthService auth, IApprovalService approvalService)
        {
            _cm = closingMonth;
            _claim = claim;
            _auth = auth;
            _apv = approvalService;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _cm.GetData(
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

        [HttpPost]
        public IActionResult OnPost(ClosingMonthRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _cm.Insert(data);

            return Ok(result);
        }

        [HttpPut]
        public IActionResult OnPut(ClosingMonthRequest data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;
            var result = _cm.Update(data);
            return Ok(result);
        }

        private (bool, string) Validate(ClosingMonthRequest data)
        {
            string filters = "";
            if (data.StartDate == null && data.EndDate == null)
            {
                var year = Convert.ToInt32(data.Period[..4]);
                var month = Convert.ToInt32(data.Period[^2..]);
                var lastDay = DateTime.DaysInMonth(year, month);
                filters = "[{\"field\":\"date\",\"operator\":\"lte\",\"keyword\":\"" + year + "-" + month + "-" + lastDay + "\"},{\"field\":\"date\",\"operator\":\"gte\",\"keyword\":\"" + year + "-" + month + "-01\"}]";
            } 
            else
            {
                var lastDay = DateTime.DaysInMonth(Convert.ToInt32(data.EndDate?.Year), Convert.ToInt32(data.EndDate?.Month));

                filters = "[{\"field\":\"date\",\"operator\":\"lte\",\"keyword\":\"" + data.EndDate?.Year + "-" + data.EndDate?.Month + "-" + lastDay + "\"},{\"field\":\"date\",\"operator\":\"gte\",\"keyword\":\"" + data.StartDate?.Year + "-" + data.StartDate?.Month + "-01\"}]";
            }

            var approvalData =
                _apv.GetData(
                    0, 1,
                    JsonConvert.DeserializeObject<List<Filter>>(filters),
                    JsonConvert.DeserializeObject<List<Sort>>("[]"),
                    null);
            if (approvalData.Data.ToDynamicList().Count > 0 && data.IsClose)
                return (false, "Tidak bisa tutup bulan karena terdapat transaksi yang belum disetujui.");

            return (true, "");
        }
    }
}
