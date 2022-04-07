using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Accounting;

[Route("is-report")]
[ApiController]
public class IncomeStatementReportController : ControllerBase
{
    private readonly IIncomeStatementReportService _isr;
    private readonly IRoleService _role;
    private readonly IClaimService _claim;

    private const int MenuId = (int)Menu.IncomeStatementReport;

    public IncomeStatementReportController(IIncomeStatementReportService isr, IRoleService role,
        IClaimService claim)
    {
        _isr = isr;
        _role = role;
        _claim = claim;
    }

    [HttpPost("lists")]
    public List<IncomeStatementResult> Listing(JournalReportModel data)
    {
        var result = new List<IncomeStatementResult>();

        if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, MenuId)).Any())
            return result;

        result = _isr.GetIncomeStatementLists(data.PeriodType, data.RptBy, data.RptDet, data.DateTo).ToList();
            
        return result;
    }

    [HttpPost("detail-lists")]
    public List<BsIsDetailResult> DetailListing(JournalReportModel data)
    {
        var result = new List<BsIsDetailResult>();

        if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, MenuId)).Any())
            return result;

        result = _isr.GetBsIsDetailLists(("IS" + data.RptBy.Substring(1, 1)), data.Acc, data.PlusMinus,
            data.DateFrom, data.DateTo, null).ToList();

        return result;
    }
}