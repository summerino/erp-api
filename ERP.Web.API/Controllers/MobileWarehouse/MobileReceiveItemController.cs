using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model;
using ERP.Web.API.Model.MobileWarehouse;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.MobileWarehouse
{
    [Route("mobile-receive-item")]
    [ApiController]
    public class MobileReceiveItemController : ControllerBase
    {
        private readonly IMobileReceiveItemService _mr;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.MobileReceiveItem;

        public MobileReceiveItemController(IMobileReceiveItemService mr, IClaimService claim, IAuthService auth)
        {
            _mr = mr;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _mr.GetData(
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
            var data = _mr.GetDetailData(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.LineNo,
                    PoDetailId = x.TransDetailId,
                    x.ItemId,
                    x.ItemInitial,
                    x.ItemName,
                    x.Qty,
                    x.UomId,
                    x.UnitId,
                    x.UnitName,
                    x.WarehouseCode,
                    x.Type,
                    OldUnitId = x.ItemUomBuyId,
                    OldUnitName = x.ItemUomBuyName,
                    OldUnitPrice = x.ItemBuyPrice,
                    TypeName = x.Type == 0 ? "Normal" : "Bonus",
                    State = ""
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, MobileReceiveItemRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _mr.Update(data);

            return Ok(result);
        }

        [HttpPut("approve")]
        public IActionResult Approve(List<MobileReceiveItemHeader> data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            var result = _mr.Approve(data, _claim.UserId);

            return Ok(result);
        }

        [HttpPut("reject")]
        public IActionResult Reject(List<MobileReceiveItemHeader> data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Reject }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            var result = _mr.Reject(data, _claim.UserId);

            return Ok(result);
        }
    }
}
