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

namespace ERP_API.Controllers.Sales
{
    [Route("api/v1/direct-invoice")]
    [ApiController]
    public class DirectInvoiceController : ControllerBase
    {
        private readonly IDirectInvoiceService _inv;
        private readonly IClaimService _claim;

        public DirectInvoiceController(IDirectInvoiceService inv, IClaimService claim)
        {
            _inv = inv;
            _claim = claim;
        }
        
        [HttpGet("{code}")]
        public IActionResult GetDataByCode(string code)
        {
            return Ok(_inv.FindByCode(code));
        }
        
        [HttpPost]
        public IActionResult OnPost(SalesInvoiceRequest data)
        {
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
            var result = _inv.Delete(code, _claim.UserId);

            return Ok(result);
        }

        private static (bool, string) Validate(SalesInvoiceRequest data)
        {
            if (!data.Details.Any())
                return (false, "Item details can't be empty.");

            return data.Details.GroupBy(x => new { x.DoCode }).Any(x => x.Count() > 1)
                ? (false, "There are duplicate receive code submitted.")
                : (true, "");
        }
    }
}
