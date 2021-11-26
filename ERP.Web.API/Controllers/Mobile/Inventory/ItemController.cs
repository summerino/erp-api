using System.Linq.Dynamic.Core;
using ERP.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity.Inventory;
using Newtonsoft.Json;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Mobile.Inventory
{
    [Authorize(AppConstant.ValidateMobileTokenPolicy)]
    [Route("mobile/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _item;
        private readonly IUnitOfMeasurementService _uom;

        public ItemController(IItemService item, IUnitOfMeasurementService uom)
        {
            _item = item;
            _uom = uom;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string warehouseCode, string filters, string sorts, int skip, int take, string lastUpdate)
        {
            var uomC = _uom.GetDataConversion().ToList();
            var data =
                _item.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    warehouseCode,
                    search, lastUpdate);
            var temp = (List<VwItem>)data.Data;
            foreach (var item in temp)
            {
                if (item.BuySeq != null)
                {
                    item.BuyQtyAvailable = (decimal)((item.QtyOnHand ?? 0 - item.QtyOnOrder ?? 0) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.BuySeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                }
                if (item.BuySeq != null)
                {
                    item.SellQtyAvailable = (decimal)((item.QtyOnHand ?? 0 - item.QtyOnOrder ?? 0) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.SellSeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                }
            }
            var result = temp.Select(x => new
            {
                x.Id,
                x.Initial,
                x.Name,
                x.CategoryId,
                x.CategoryName,
                x.TypeId,
                x.TypeName,
                x.QtyOnHand,
                x.QtyOnIndent,
                x.SellQtyAvailable,
                x.SellPrice,
                x.UomId,
                x.UomSellId,
                x.UomSellName,
                x.BuyQtyAvailable,
                x.BuyPrice,
                x.UomBuyId,
                x.UomBuyName,
                x.SalesTaxId,
                x.IsActive,
                x.UpdatedDate
            }).ToList<dynamic>();
            return Ok(new MobileApiResponse
            {
                Count = data.Total,
                Data = result
            });
        }

        [HttpGet("uomConversion")]
        public IActionResult GetUomConversion(int? uomId=null)
        {
            var data = _uom.GetDataConversion(uomId);
            
            return Ok(data);
        }

        [HttpGet("uom")]
        public IActionResult GetUom(string lastUpdate)
        {
            var data =
                _uom.GetLists(null, null, lastUpdate).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Initial,
                        x.BaseUnit,
                        x.Description,
                        x.IsActive,
                        x.UpdatedDate
                    })
                    .ToList<dynamic>();

            return Ok(data);
        }
        
        [HttpGet("info")]
        public IActionResult GetItemInformation(int itemId, string custCode)
        {
            var data = _item.GetItemInformation(itemId,custCode);

            return Ok(data);
        }
    }
}
