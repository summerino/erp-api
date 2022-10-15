using System.Data;
using System.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces;

namespace ERP.Web.API.Domain.Services;

public class LocalReportService : ILocalReportService
{
    private readonly CatalogContext _catalogCtx;
    private readonly IClaimService _claim;
    private readonly IWebHostEnvironment _env;

    public LocalReportService(CatalogContext catalogCtx, IClaimService claim,
        IWebHostEnvironment env)
    {
        _catalogCtx = catalogCtx;
        _claim = claim;
        _env = env;
    }

    public byte[] GeneratePdf(string reportName, string[] codes)
    {
        // Get tenant server information
        var tenant = _catalogCtx.Tenants.Find(_claim.TenantId);
        if (tenant == null)
            return Array.Empty<byte>();

        // Set query
        var query = reportName switch
        {
            "sales-invoice-multi" => $@"
SELECT TOP 1 Name, Address1, Address2, Phone, Fax
FROM SystemManagement.Company;

EXEC sp_get_si_multi_print_data '{string.Join(",", codes)}', 1;",

            "delivery-order-multi" => $@"
SELECT TOP 1 Name, Address1, Address2, Phone, Fax
FROM SystemManagement.Company;

EXEC sp_get_do_multi_print_data '{string.Join(",", codes)}';",

            _ => null
        };

        if (string.IsNullOrEmpty(query))
            return Array.Empty<byte>();

        // Populate DataSet
        var ds = new DataSet();
        using var cn = new SqlConnection($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword};Application Name=ERP");
        cn.Open();

        using var da = new SqlDataAdapter(query, cn);
        da.SelectCommand.CommandTimeout = 600;
        da.Fill(ds);

        var report = new LocalReport();

        // Load report definition
        using var fs =
            new FileStream($"{_env.WebRootPath}/printout/{_claim.TenantInitial.ToLower()}/{reportName}.rdl", FileMode.Open);
        report.LoadReportDefinition(fs);

        // Adding data source
        if (ds.Tables.Count == 2)
        {
            report.DataSources.Add(new ReportDataSource("Company", ds.Tables[0]));
            report.DataSources.Add(new ReportDataSource("Data", ds.Tables[1]));
        }

        // Adding parameter
        var parameters = new[] { new ReportParameter("code", codes) };
        report.SetParameters(parameters);

        // Convert to pdf process
        return report.Render("PDF");
    }
}