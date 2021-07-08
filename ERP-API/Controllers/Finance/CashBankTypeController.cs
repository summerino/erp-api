using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Domain.Interfaces.Finance;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Domain.Services;
using ERP_API.Model;
using ERP_API.Model.Finance;
using ERP_API.Model.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace ERP_API.Controllers.Finance
{
    [Route("api/v1/general-cash-bank-type")]
    [ApiController]
    [AllowAnonymous]
    public class CashBankTypeController : ControllerBase
    {
        private readonly ICashBankTypeService _cashBankType;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int _menuId = (int)Menu.AccountPayable;
        public CashBankTypeController(ICashBankTypeService cashBank, IClaimService claim, IAuthService auth)
        {
            _cashBankType = cashBank;
            _claim = claim;
            _auth = auth;
        }
        [HttpGet("lists")]
        public IActionResult GetLists()
        {
            var data = _cashBankType.GetList();
            return Ok(data);
            //return Ok(new ApiResponse
            //{
            //    RowCount = data.Count,
            //    TableData = data
            //});
        }

    }
}
