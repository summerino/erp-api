using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.MobileSales
{
    [Route("mobile-visit-log")]
    [ApiController]
    public class MobileVisitLogController : ControllerBase
    {
        private readonly IMobileVisitLogService _vl;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.MobileVisitLog;

        public MobileVisitLogController(IMobileVisitLogService vl, IClaimService claim, IAuthService auth)
        {
            _vl = vl;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _vl.GetData(
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

        [HttpPut("approve")]
        public IActionResult Approve(List<MobileVisitLog> data)
        {
            //if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}
            var result = _vl.Approve(data, _claim.UserId);

            return Ok(result);
        }

        [HttpPut("reject")]
        public IActionResult Reject(List<MobileVisitLog> data)
        {
            //if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}
            var result = _vl.Reject(data, _claim.UserId);

            return Ok(result);
        }
    }
}
