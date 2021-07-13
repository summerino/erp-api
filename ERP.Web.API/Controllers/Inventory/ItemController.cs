using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _item;
        private readonly IUnitOfMeasurementService _uom;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.Item;
        public ItemController(IItemService item, IUnitOfMeasurementService uom, IClaimService claim, IAuthService auth)
        {
            _uom = uom;
            _item = item;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string category, string warehouseCode, string filters, string sorts, int skip, int take)
        {
            var uomC = _uom.GetDataConversion().ToList();
            var data =
                _item.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<int>>(!string.IsNullOrWhiteSpace(category) ? category : "[]"),
                    warehouseCode,
                    search);
            var result = (List<VwItem>)data.Data;
            foreach (var item in result)
            {
                MapNull(item);

                if (item.BuySeq != null)
                {
                    item.BuyQtyAvailable = (decimal)((item.QtyOnHand - item.QtyOnOrder) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.BuySeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                }
                if (item.BuySeq != null)
                {
                    item.SellQtyAvailable = (decimal)((item.QtyOnHand - item.QtyOnOrder) / uomC.Where(u => u.UomId.Equals(item.UomId) && u.Seq <= item.SellSeq).Select(x => x.Conversion).Aggregate((a, x) => a * x));
                }
            }
            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = result.ToDynamicList()
            });
        }

        [HttpGet("related")]
        public IActionResult GetOrderTrans(string whid, int itemid, int from)
        {
            List<dynamic> data = new();
            if (from == 1)
            {
                data = _item.GetRelatedOrderTrans(whid, itemid).ToList<dynamic>();
            }
            else if (from == 2)
            {
                data = _item.GetRelatedIndentTrans(whid, itemid).ToList<dynamic>();
            }
            else
            {
                data = _item.GetRelatedTransferTrans(whid, itemid).ToList<dynamic>();
            }

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(Item data)
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

            var result = _item.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, Item data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _item.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _item.Delete(id, _claim.UserId);
            return Ok(result);
        }

        private void MapNull(VwItem data) 
        {
            if (data.QtyOnHand == null)
                data.QtyOnHand = 0;

            if (data.QtyOnIndent == null)
                data.QtyOnIndent = 0;

            if (data.QtyOnOrder == null)
                data.QtyOnOrder = 0;

            if (data.QtyOnTransfer == null)
                data.QtyOnTransfer = 0;
        }
    }
}
