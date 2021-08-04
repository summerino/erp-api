using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model;
using ERP.Web.API.Model.General;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.General
{
    [Route("approval")]
    [ApiController]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approval;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int _menuId = (int)Menu.Approval;

        public ApprovalController(IApprovalService approval, IClaimService claim, IAuthService auth)
        {
            _approval = approval;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _approval.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    _auth.GetActions(_menuId, _claim.RoleId, new List<int>()).ToList(),
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }


        [HttpPost]
        public IActionResult OnPost(List<ApprovalRequest> data)
        {
            var actionIdLists = data.GroupBy(x => x.ActionId).Select(x => x.Key).ToList();

            if (_auth.GetActions(_menuId, _claim.RoleId, actionIdLists).Count() != actionIdLists.Count)
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            var result = _approval.SaveChanges(data, _claim.UserId);

            return Ok(result);
        }
    }
}
