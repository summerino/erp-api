using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Accounting;

[Route("bs-report")]
[ApiController]
public class BalanceSheetController : ControllerBase
{
    private readonly IGeneralLedgerReportService _gl;
    private readonly IBalanceSheetReportService _bs;
    private readonly IRoleService _role;
    private readonly IClaimService _claim;

    private const int MenuId = (int)Menu.BalanceSheetReport;

    public BalanceSheetController(IBalanceSheetReportService bs,
        IGeneralLedgerReportService gl,
        IRoleService role, IClaimService claim)
    {
        _bs = bs;
        _gl = gl;
        _claim = claim;
        _role = role;
    }

    [HttpPost("lists")]
    public IActionResult Listing(JournalReportModel data)
    {
        var firstDay = new DateTime(data.Date.Year, data.Date.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var details = _gl.GetGeneralLedgerLists(firstDay.ToString(), lastDay.ToString(), data.Acc, data.Acc2,
            "IDR", "N", null).Where(x => x.Sort == "3");

        var result = _bs.GetBalanceSheetLists(details).ToList<dynamic>();

        return Ok(new ApiResponse
        {
            RowCount = result.Count(),
            TableData = result
        });
    }
}