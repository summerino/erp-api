using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileSales;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileSales
{
    [Route("mobile-sales-cost")]
    [ApiController]
    public class MobileCostController : ControllerBase
    {
        private readonly IMobileCostService _mc;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.MobileSalesCost;

        public MobileCostController(IMobileCostService mc, IClaimService claim, IAuthService auth)
        {
            _mc = mc;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _mc.GetData(
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
            var data = _mc.GetDetailData(code)
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("image")]
        public IActionResult GetImageData(string code)
        {
            var data = _mc.GetImageData(code)
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPut("approve")]
        public IActionResult Approve(List<MobileCostRequest> data, string date, string coa, string notes)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _mc.Approve(data, _claim.UserId, date, coa, notes);

            return Ok(result);
        }

        [HttpPut("reject")]
        public IActionResult Reject(List<MobileCostRequest> data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _mc.Reject(data, _claim.UserId);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, MobileCostRequest data)
        {

            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _mc.Update(data);

            return Ok(result);
        }
    }
}
