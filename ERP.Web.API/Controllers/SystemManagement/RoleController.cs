using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.SystemManagement;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.SystemManagement
{
    [Route("[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _role;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int MenuId = (int)Menu.Role;

        public RoleController(IRoleService role, IClaimService claim, IAuthService auth)
        {
            _role = role;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _role.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("role-menus")]
        public IActionResult GetRoleMenu(int id)
        {
            return Ok(_role.GetRoleMenu(id));
        }

        [HttpGet("role-menu-actions")]
        public IActionResult GetRoleMenuAction(int id)
        {
            var data = _role.GetRoleMenuAction(id)
                .Select(x => new
                {
                    x.Id,
                    x.MenuId,
                    x.ActionId,
                    x.UpdatedBy,
                    x.UpdatedDate
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("get-action")]
        public IActionResult GetAction(int menuId, string actions)
        {
            var result =
                _auth.GetActions(
                    menuId,
                    _claim.RoleId,
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(actions) ? actions : "[]")
                );

            return Ok(result);
        }

        [HttpPost]
        public IActionResult OnPost(RoleRequest data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            data.IsActive = true;
            data.CreatedBy = 1;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _role.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(int id, RoleRequest data)
        {

            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _role.Update(data, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {

            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _role.Delete(id, 1);

            return Ok(result);
        }
    }
}
