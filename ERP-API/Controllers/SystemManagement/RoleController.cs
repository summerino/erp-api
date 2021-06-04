using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.SystemManagement;
using Newtonsoft.Json;

namespace ERP_API.Controllers.SystemManagement
{
    [Route("api/v1/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _role;
        private readonly IClaimService _claim;

        public RoleController(IRoleService role, IClaimService claim)
        {
            _role = role;
            _claim = claim;
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

        [HttpPost]
        public IActionResult OnPost(RoleRequest data)
        {
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
            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _role.Update(data, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _role.Delete(id, 1);

            return Ok(result);
        }
    }
}
