using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace ERP_API.Controllers.General
{
    [Route("payment-term")]
    [ApiController]
    public class PaymentTermController : ControllerBase
    {
        private readonly IPaymentTermService _paymentTerm;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        public PaymentTermController(IPaymentTermService paymentTermService, IClaimService claimService, IAuthService authService)
        {
            _paymentTerm = paymentTermService;
            _claim = claimService;
            _auth = authService;
        }

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts)
        {
            var data =
                _paymentTerm.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Initial,
                        x.Name,
                        x.Due
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _paymentTerm.GetData(
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
        public IActionResult OnPost(PaymentTerm data)
        {

            //if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}

            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _paymentTerm.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, PaymentTerm data)
        {

            //if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _paymentTerm.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(int id)
        {
            //if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}

            var result = _paymentTerm.Delete(id, _claim.UserId);
            return Ok(result);
        }
    }
}
