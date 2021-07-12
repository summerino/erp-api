using ERP_API.Domain.Interfaces.Auth;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP.Entity;
using ERP_API.Model;
using ERP_API.Model.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace ERP_API.Controllers.General
{
    [Route("approval")]
    [ApiController]
    [AllowAnonymous]
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
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Approve }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _approval.SaveChanges(data, _claim.UserId);
            return Ok(result);
        }

    }
}
