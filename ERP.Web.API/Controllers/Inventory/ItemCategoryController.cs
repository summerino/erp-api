using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("item-category")]
    //[Authorize]
    [ApiController]
    public class ItemCategoryController : ControllerBase
    {
        private readonly IItemCategoryService _category;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.ItemCategory;

        public ItemCategoryController(IItemCategoryService category, IClaimService claim, IAuthService auth)
        {
            _category = category;
            _auth = auth;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _category.GetData(
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

        [HttpGet("hierarchy")]
        public IActionResult GetHierarchy()
        {
            return Ok(_category.GetHierarchy());
        }

        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _category.GetLists()
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.Name,
                    x.ParentId,
                    x.GroupId,
                    x.Seq,
                    x.Deep,
                    x.Lineage
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(ItemCategory data)
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

            var result = _category.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(int id, ItemCategory data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = 1;
            data.UpdatedDate = DateTime.Now;

            var result = _category.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _category.Delete(id, 1);
            return Ok(result);
        }
    }
}
