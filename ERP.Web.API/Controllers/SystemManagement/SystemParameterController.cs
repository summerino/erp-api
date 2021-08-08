using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.SystemManagement;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.SystemManagement
{
    [Route("system-parameter")]
    [ApiController]
    public class SystemParameterController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ISystemParameterService _sysParam;
        private readonly IClaimService _claim;
        private const int _menuId = (int)Menu.Parameter;

        public SystemParameterController(ISystemParameterService sysParam, IClaimService claimService, IAuthService auth)
        {
            _sysParam = sysParam;
            _claim = claimService;
            _auth = auth;
        }
       
        [HttpGet]
        public IActionResult GetHierarchy()
        {
            var data = _sysParam.GetHierarchy();
            return Ok(data);
        }

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts, string codes) 
        {
            var data =
                _sysParam.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    JsonConvert.DeserializeObject<List<string>>(!string.IsNullOrWhiteSpace(codes) ? codes : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Code, x.Value
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }
        [HttpPost]
        public IActionResult OnPost(List<SystemParameterRequest> data)
        {
            //if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            //{
            //    return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            //}
            var result = _sysParam.Save(data);
            return Ok(result);
        }

    }
}
