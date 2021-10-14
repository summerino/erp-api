using System.Linq;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Services.Mobile.General
{
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
            var dataOrder = from so in Db.SalesOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.Date.Month.Equals(month) &&
                             x.SalesBy.Equals(salesId)).AsEnumerable()
                            group so by new { so.Date.Year, so.Date.Month } into g
                            select new VisitInformationModel
                            {
                                Total = g.Sum(x => x.Total)
                            };

            var dataMobile = from so in Db.MobileOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.Date.Month.Equals(month) &&
                              x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId)).AsEnumerable()
                             group so by new { so.Date.Year, so.Date.Month } into g
                             select new VisitInformationModel
                             {
                                 Total = g.Sum(x => x.Total)
                             };

            var data = (from so in dataOrder.Union(dataMobile).AsEnumerable()
                        group so by new { so.Total } into g
                        select new VisitInformationModel
                        {
                            Total = g.Sum(x => x.Total)
                        }).SingleOrDefault();
            if (data != null)
            {
                return data;
            }

            return new VisitInformationModel
            {
                Total = 0
            }; ;
        }
    }
}
