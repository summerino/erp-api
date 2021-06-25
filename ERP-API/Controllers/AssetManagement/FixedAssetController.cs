using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Newtonsoft.Json;
using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Domain.Interfaces.AssetManagement;
using ERP_API.Domain.Entities.AssetManagement;

namespace ERP_API.Controllers.AssetManagement
{
    [Route("api/v1/fixed-asset")]
    [ApiController]
    public class FixedAssetController : ControllerBase
    {
        private readonly IClaimService _claim;
        private readonly IFixedAssetService _fixedAsset;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.FixedAsset;

        public FixedAssetController(IFixedAssetService fixedAsset, IAuthService auth, IClaimService claim)
        {
            _auth = auth;
            _claim = claim;
            _fixedAsset = fixedAsset;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _fixedAsset.GetData(
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
        public IActionResult OnPost(FixedAsset data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Insert process
            data.Mark = "A";
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _fixedAsset.Insert(data);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string code, FixedAsset data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _fixedAsset.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(string code)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Void }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _fixedAsset.Delete(code, _claim.UserId);
            return Ok(result);
        }
    }
}
