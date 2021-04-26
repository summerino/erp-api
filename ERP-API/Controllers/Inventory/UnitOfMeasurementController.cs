using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Inventory;
using Newtonsoft.Json;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/uom")]
    [ApiController]

    public class UnitOfMeasurementController : ControllerBase
    {
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;

        public UnitOfMeasurementController(IUnitOfMeasurementService uom, IClaimService claim)
        {
            _uom = uom;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string filters, string sorts, int skip, int take)
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

        [HttpGet("item")]
        public IActionResult GetDetail(int uomId)
        {
            var uomC = _uom.GetDataConversion(uomId);
            var data = uomC.ToList<dynamic>();
            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(UnitOfMeasurementRequest data)
        {

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
            var result = _uom.Delete(id, _claim.UserId);
            return Ok(result);
        }

        private static (bool, string) Validate(UnitOfMeasurementRequest data)
        {
            //validate if items empty
            if (!data.Details.Any())
                return (false, "Item details can't be empty.");
            
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
            while (true) {
                if (index == details.Length)
                    break;

                var currentItem = details[index];
                var listToCompare = details.Where(x => x.Seq != currentItem.Seq).ToList();
                if (listToCompare.Any(x=>x.UnitEquivalent == currentItem.UnitEquivalent && x.Conversion == currentItem.Conversion)) { 
                    return (false, $"Cannot add duplicate item.");
                }
                index++;
            }
            return (true,"");
        }
    }
}
