namespace ERP.Web.API.Model;

public class LocalReportRequest
{
    public string ReportName { get; set; }

    public IEnumerable<string> Codes { get; set; }
}