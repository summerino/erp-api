using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Inventory;

[Route("sm-report")]
[ApiController]
public class SmReportController : ControllerBase
{
    private readonly ISMReportService _sm;
    private readonly IClaimService _claim;
    private readonly IAuthService _auth;
    private const int MenuId = (int)Menu.StockMutationReport;
    public SmReportController(ISMReportService sm, IClaimService claim, IAuthService auth)
    {
        _sm = sm;
        _claim = claim;
        _auth = auth;
    }

    [HttpGet]
    public IActionResult GetData(int type, string startDate, string endDate, string whCode, int? itemId, int typeUnit, bool isSm, string sorts)
    {
        var result = _sm.GetData(type, startDate, endDate, whCode, itemId, typeUnit, isSm);

        var tableData = result.Data.ToDynamicList();
        if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.ShowInventoryValue }).Any())
        {
            foreach (var item in tableData)
            {
                if (type == 1)
                {
                    if (isSm)
                    {
                        var data = (ReportByStockMutation)item;
                        data.InvEnd = 0;
                        data.InvIn = 0;
                        data.InvIn = 0;
                    }
                    else
                    {
                        var data = (ReportByItem)item;
                        data.InvBegin = 0;
                        data.InvEnd = 0;
                        data.InvIn = 0;
                        data.InvIn = 0;
                    }
                }
                else
                {
                    var data = (ReportByWarehouse)item;
                    data.InvBegin = 0;
                    data.InvEnd = 0;
                    data.InvIn = 0;
                    data.InvIn = 0;
                }
            }
        }
        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = tableData
        });
    }
}