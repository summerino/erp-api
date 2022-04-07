using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Services.Mobile.General;

public class VisitInformationService : IVisitInformationService
{
    protected TenantContext Db;

    public VisitInformationService(TenantContext db)
    {
        Db = db;
    }

    public VisitInformationModel GetVisitInformation(int year, int month, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.Date.Month.Equals(month) && x.SalesBy.Equals(salesId))
            group so by new { so.Date.Year, so.Date.Month } into g
            select new VisitInformationModel
            {
                Date = g.Key.Month,
                Total = g.Sum(x => x.Total)
            });

        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.Date.Month.Equals(month) && x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId))
            group so by new { so.Date.Year, so.Date.Month } into g
            select new VisitInformationModel
            {
                Date = g.Key.Month,
                Total = g.Sum(x => x.Total)
            });

        var data = (from so in dataOrder.Union(dataMobile)
            group so by new { so.Date, so.Total } into g
            select new VisitInformationModel
            {
                Date = g.Key.Date,
                Total = g.Sum(x => x.Total)
            }).ToList();

        decimal Total = 0;
        foreach (var so in data)
        {
            Total += so.Total;
        }

        if (data != null)
        {
            return new VisitInformationModel
            {
                Total = Total
            };
        }

        return new VisitInformationModel
        {
            Total = 0
        };
    }
}