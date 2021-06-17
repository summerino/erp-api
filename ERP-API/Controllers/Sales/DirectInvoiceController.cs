using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Sales;
using Newtonsoft.Json;
using ERP_API.Domain.Interfaces.Auth;

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/direct-invoice")]
    [ApiController]
    public class DirectInvoiceController : ControllerBase
    {
        private readonly IDirectInvoiceService _inv;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.DirectInvoice;
        public DirectInvoiceController(IDirectInvoiceService inv, IClaimService claim, IAuthService auth)
        {
            _inv = inv;
            _claim = claim;
            _auth = auth;
        }
        
        [HttpGet("{code}")]
        public IActionResult GetDataByCode(string code)
        {
            return Ok(_inv.FindByCode(code));
        }
        
        [HttpPost]
        public IActionResult OnPost(SalesInvoiceRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Insert process
            data.Mark = "A";
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _inv.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, SalesInvoiceRequest data)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            // Validate process
            var (isValid, message) = Validate(data);
            if (!isValid)
                return Ok(new SaveResult(false, message));

            // Update process
            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _inv.Update(data);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {

            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _inv.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesInvoiceRequest data)
        {
            if (!data.ItemDetails.Any())
                return (false, "Item details can't be empty.");

            return data.ItemDetails.GroupBy(x => new { x.ItemId }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate receive code submitted.")
                : (true, "");
        }
    }
}
