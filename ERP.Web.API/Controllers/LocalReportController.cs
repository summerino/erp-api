using Microsoft.AspNetCore.Mvc;
using ERP.Web.API.Domain.Interfaces;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers;

[Route("local-report")]
[ApiController]
public class LocalReportController : ControllerBase
{
    private readonly ILocalReportService _report;

    public LocalReportController(ILocalReportService report)
    {
        _report = report;
    }

    [HttpPost]
    public IActionResult OnPost(LocalReportRequest data)
    {
        var pdfBytes = _report.GeneratePdf(data.ReportName, data.Codes.ToArray());
        return File(pdfBytes, "application/pdf", $"{data.ReportName}.pdf");
    }
}
