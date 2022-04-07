namespace ERP.Web.API.Domain.Interfaces;

public interface ICustomDynamicReportService
{
    public dynamic GetDataReport(int? id, string param1, string param2,
        string param3, string param4, string param5);

    public dynamic GetDataSource(string source);
}