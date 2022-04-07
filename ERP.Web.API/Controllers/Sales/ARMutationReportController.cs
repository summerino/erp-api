using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales;

[Route("ar-mutation-report")]
[ApiController]
public class ARMutationReportController : ControllerBase
{
    private readonly IARMutationReportService _arm;
    public ARMutationReportController(IARMutationReportService arm)
    {
        _arm = arm;
    }

    [HttpGet]
    public IActionResult GetData(int type, string startDate, string endDate, string custCode, int slsId, string status)
    {
        var result =
            _arm.GetData(type, startDate, endDate, custCode, slsId, status);

        return Ok(new ApiResponse
        {
            RowCount = result.Total,
            TableData = result.Data.ToDynamicList()
        });
    }
}