using ERP.Common;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Sales;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("credit-limit-approval")]
    [ApiController]
    public class CreditLimitApprovalController : ControllerBase
    {
        private readonly ICreditLimitApprovalService _approval;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private const int MenuId = (int)Menu.Approval;

        public CreditLimitApprovalController(ICreditLimitApprovalService approval, IClaimService claim, IAuthService auth)
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
        public IActionResult OnPost(List<SalesOrderRequest> data)
        {
            // Checking role authorization
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Approve }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            var result = _approval.SaveChanges(data, _claim.UserId);

            return Ok(result);
        }
    }
}
