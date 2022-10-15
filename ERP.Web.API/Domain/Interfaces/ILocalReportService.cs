namespace ERP.Web.API.Domain.Interfaces;

public interface ILocalReportService
{
    byte[] GeneratePdf(string reportName, string[] codes);
}