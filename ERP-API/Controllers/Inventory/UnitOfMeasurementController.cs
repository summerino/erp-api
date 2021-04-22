using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Newtonsoft.Json;
using Swift.Framework.Model;
using ERP_API.Model.Inventory;
using System.Linq;
using ERP_API.Domain.Services;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/uom")]
    //[Authorize]
    [ApiController]
    public class UnitOfMeasurementController : ControllerBase
    {
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;
        private readonly int _userId;

        public UnitOfMeasurementController(IUnitOfMeasurementService uom, IClaimService claim)
        {
            _uom = uom;
            _claim = claim;
            _userId = claim.UserId;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
        {
            var data =
                _uom.GetData(
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

        [HttpPost]
        public IActionResult OnPost(UnitOfMeasurementRequest data)
        {

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));
            var result = _uom.Insert(data, _userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, UnitOfMeasurementRequest data)
        {
            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));
            var result = _uom.Update(data, _userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            var result = _uom.Delete(id, _userId);
            return Ok(result);
        }

        [HttpGet("item")]
        public IActionResult GetDetail(int id)
        {
            var uomC = _uom.GetDataConversion(id);
            var data = uomC.ToList<dynamic>();
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        private static (bool, string) Validate(UnitOfMeasurementRequest data)
        {
            if (!data.Details.Any())
                return (false, "Item details can't be empty.");

            short index = 0;
            bool finish = false;
            var details = data.Details.ToArray();
            while (!finish)
            {
                if (index == details.Count())
                    break;
                if (index > 0) 
                {
                    string currentUnitToConvert = details[index].UnitToConvert;
                    string lastUnitToConvert = details[index - 1].UnitEquivalent;
                    if (currentUnitToConvert != lastUnitToConvert) 
                    {
                        finish = true;
                        return (false, $"Item Conversion {currentUnitToConvert} is not match.");
                    }
                }
                index++;
            }
            return (true,"");
        }

    }
}
