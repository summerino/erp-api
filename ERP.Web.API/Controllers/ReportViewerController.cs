using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using BoldReports.Web;
using BoldReports.Web.ReportViewer;
using ERP.Web.API.Domain.Interfaces.Catalog;
using ERP.Entity;

namespace ERP.Web.API.Controllers;

[Route("report-viewer/[action]")]
[ApiController]
public class ReportViewerController : ControllerBase, IReportController
{
    private readonly ITenantService _tenant;
    private readonly IClaimService _claim;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, object> _jsonArray;

    public ReportViewerController(ITenantService tenant, IClaimService claim,
        IMemoryCache cache, IWebHostEnvironment env)
    {
        _tenant = tenant;
        _claim = claim;
        _cache = cache;
        _env = env;
    }

    // Post action to process the report from server based json parameters and send the result back to the client.
    [HttpPost]
    public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
    {
        if (jsonArray != null && jsonArray.Keys.Count > 0)
        {
            _jsonArray = jsonArray;
        }
        return ReportHelper.ProcessReport(jsonArray, this, _cache);
    }

    // Method will be called to initialize the report information to load the report with ReportHelper for processing.
    [NonAction]
    public void OnInitReportOptions(ReportViewerOptions reportOption)
    {
        reportOption.ReportModel.EmbedImageData = true;

        var reportStream =
            new FileStream(
                $@"{_env.WebRootPath}\printout\{_claim.TenantInitial.ToLower()}\{reportOption.ReportModel.ReportPath}",
                FileMode.Open, FileAccess.Read);

        reportOption.ReportModel.Stream = reportStream;
    }

    // Method will be called when reported is loaded with internally to start to layout process with ReportHelper.
    [NonAction]
    public void OnReportLoaded(ReportViewerOptions reportOption)
    {
        if (_jsonArray != null && _jsonArray.Keys.Count > 0)
        {
            // Get tenant server information
            var tenant = _tenant.FindById(_claim.TenantId);

            if (tenant != null)
            {
                // Re-define data source connection string
                var datasources = ReportHelper.GetDataSources(_jsonArray, this, _cache, true);
                foreach (DataSourceInfo item in datasources)
                {
                    var dataSourceCredentials = new DataSourceCredentials
                    {
                        Name = item.DataSourceName,
                        UserId = tenant.ServerUserId,
                        Password = tenant.ServerPassword,
                        ConnectionString = $"Data Source={tenant.ServerName};Initial Catalog={tenant.DatabaseName};TrustServerCertificate=True;Application Name=ERP",
                        IntegratedSecurity = false
                    };

                    reportOption.ReportModel.DataSourceCredentials = new List<DataSourceCredentials>
                    {
                        dataSourceCredentials
                    };
                }
            }
        }
    }

    //Get action for getting resources from the report
    [ActionName("GetResource")]
    [AcceptVerbs("GET")]
    [AllowAnonymous]
    // Method will be called from Report Viewer client to get the image src for Image report item.
    public object GetResource(ReportResource resource)
    {
        return ReportHelper.GetResource(resource, this, _cache);
    }

    [HttpPost]
    [AllowAnonymous]
    public object PostFormReportAction()
    {
        return ReportHelper.ProcessReport(null, this, _cache);
    }
}