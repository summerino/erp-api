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
    [Route("uom")]
    [ApiController]

    public class UnitOfMeasurementController : ControllerBase
    {
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.Uom;

        public UnitOfMeasurementController(IUnitOfMeasurementService uom, IClaimService claim, IAuthService auth)
        {
            _uom = uom;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _uom.GetData(
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
        public IActionResult GetList(string sorts) 
        {
            var data =
                _uom.GetLists(
                    null,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id, x.Initial, x.BaseUnit
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("item")]
        public IActionResult GetDetailData(int? uomId)
        {
            var data = _uom.GetDataConversion(uomId).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(UnitOfMeasurementRequest data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));
            var result = _uom.Insert(data, _claim.UserId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, UnitOfMeasurementRequest data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));
            var result = _uom.Update(data, _claim.UserId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _uom.Delete(id, _claim.UserId);
            return Ok(result);
        }

        private static (bool, string) Validate(UnitOfMeasurementRequest data)
        {
            //validate if items empty
            if (!data.Details.Any())
                return (false, "Detail tidak boleh kosong.");
            
            //validate if items rules is not match
            short index = 0;
            var details = data.Details.ToArray();
            while (true)
            {
                if (index == details.Length)
                    break;
                if (index > 0) 
                {
                    string currentUnitToConvert = details[index].UnitToConvert;
                    string lastUnitToConvert = details[index - 1].UnitEquivalent;
                    if (currentUnitToConvert != lastUnitToConvert) 
                    {
                        return (false, $"Item Conversion {currentUnitToConvert} is not match.");
                    }
                }
                index++;
            }

            //validate duplicate item
            index = 0;
            while (true)
            {
                if (index == details.Length)
                    break;

                var currentItem = details[index];
                var listToCompare = details.Where(x => x.Seq != currentItem.Seq).ToList();
                if (listToCompare.Any(x => x.UnitEquivalent.Equals(currentItem.UnitEquivalent, System.StringComparison.OrdinalIgnoreCase) && x.Conversion == currentItem.Conversion))
                {
                    return (false, $"Cannot add duplicate item.");
                }
                index++;
            }
            return (true,"");
        }
    }
}
