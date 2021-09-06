using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("tb-report")]
    [ApiController]
    public class TrialBalanceReportController : ControllerBase
    {
        private readonly ITrialBalanceReportService _tb;
        private readonly IRoleService _role;
        private readonly IClaimService _claim;

        private const int MenuId = (int)Menu.TrialBalanceReport;

        public TrialBalanceReportController(ITrialBalanceReportService tb, IRoleService role, IClaimService claim)
        {
            _tb = tb;
            _role = role;
            _claim = claim;
        }

        [HttpPost("lists")]
        public List<TrialBalanceResult> Listing(JournalReportModel data)
        {
            var result = new List<TrialBalanceResult>();

            if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, MenuId)).Any())
                return result;

            result = _tb.GetTrialBalanceLists(data.RptBy, data.DateFrom, data.DateTo, data.Curr).ToList();

            return result;
        }
    }
}
