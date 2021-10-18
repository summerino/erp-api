using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileWarehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.MobileWarehouse
{
    [Route("mobile-delivery-item")]
    [ApiController]
    public class MobileDeliveryItemController : ControllerBase
    {
        private readonly IMobileDeliveryItemService _md;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.MobileDeliveryItem;

        public MobileDeliveryItemController(IMobileDeliveryItemService md, IClaimService claim, IAuthService auth)
        {
            _md = md;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _md.GetData(
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
            var data = _md.GetDetailData(code)
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, MobileDeliveryItemRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _md.Update(data);

            return Ok(result);
        }

        [HttpPut("approve")]
        public IActionResult Approve(List<MobileDeliveryItemHeader> data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            var result = _md.Approve(data, _claim.UserId);

            return Ok(result);
        }

        [HttpPut("reject")]
        public IActionResult Reject(List<MobileDeliveryItemHeader> data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            var result = _md.Reject(data, _claim.UserId);

            return Ok(result);
        }
    }
}
