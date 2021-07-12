using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.AssetManagement;
using ERP.Web.API.Domain.Interfaces.AssetManagement;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.AssetManagement
{
    [Route("asset-type")]
    [ApiController]
    public class AssetTypeController : ControllerBase
    {
        private readonly IClaimService _claim;
        private readonly IAssetTypeService _assetType;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.AssetType;

        public AssetTypeController(IAssetTypeService assetType, IAuthService auth, IClaimService claim)
        {
            _auth = auth;
            _claim = claim;
            _assetType = assetType;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _assetType.GetData(
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

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts) 
        {
            var data =
                _assetType.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id, x.Initial, x.Name, x.CoaDeprecExpense, x.CoaAccumDeprec, x.CoaAsset, x.CoaExpense
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(AssetType data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _assetType.Insert(data);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, AssetType data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _assetType.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _assetType.Delete(id);
            return Ok(result);
        }
    }
}
