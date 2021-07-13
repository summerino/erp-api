using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Inventory;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("item-group")]
    [ApiController]
    public class ItemGroupController : ControllerBase
    {
        private readonly IItemGroupService _itemGroup;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.ItemGroup;

        public ItemGroupController(IItemGroupService itemGroup, IClaimService claim, IAuthService auth)
        {
            _itemGroup = itemGroup;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _itemGroup.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    null,
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("item")]
        public IActionResult GetDetailData(int id)
        {
            var data = _itemGroup.GetDetailData(id)
                .Select(x => new
                {
                    x.Id,
                    x.ItemGroupId,
                    x.Name,
                    x.Value
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("lists")]
        public IActionResult GetList()
        {
            var data = _itemGroup.GetLists()
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.Name
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("item-by-id")]
        public IActionResult GetDetailById(int id)
        {
            var data = _itemGroup.GetDetailById(id)
                .Select(x => new
                {
                    x.Id,
                    x.ItemGroupId,
                    x.Name,
                    x.Value
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(ItemGroupRequest data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _itemGroup.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(int id, ItemGroupRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _itemGroup.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _itemGroup.Delete(id, 1);
            return Ok(result);
        }
    }
}
