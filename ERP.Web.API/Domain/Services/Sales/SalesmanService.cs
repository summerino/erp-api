using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class SalesmanService : GeneralService<SalesmanGroup>, ISalesmanService
    {
        public SalesmanService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwSalesmanGroups.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search) || x.SupervisorInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(string groupId, string startDate, string recurrence, string visitDay)
        {
            var data = Db.VwSalesmanSchedules.AsQueryable();

            if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(recurrence) && !string.IsNullOrEmpty(visitDay))
            {
                if (DateTime.TryParse(startDate, out var outStartDate))
                {
                    data = data.Where(x => x.VisitDay == Convert.ToByte(visitDay) && x.SalesGroupId == Convert.ToInt32(groupId)).Where(x => outStartDate >= x.StartDate).Where(x => outStartDate <= x.EndDate);
                }

                return data.OrderBy(x => x.Id);
            }

            return Db.VwSalesmanSchedules.Where(x => x.Id == 0);
        }

        public IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(long id)
        {
            var data = Db.VwSalesmanSchedules.Where(x => x.Id == id);

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<VwSalesmanScheduleCustomer> GetSalesmanScheduleDetailData(List<long> id)
        {
            var data = Db.VwSalesmanScheduleCustomers.AsQueryable();

            if (id?.Any() ?? false)
            {
                data = Db.VwSalesmanScheduleCustomers.Where(x => id.Contains(x.SalesmanScheduleId));
            }

            return data.OrderBy(x => x.SalesmanScheduleId);
        }

        #region Mobile
        public SalesProfile GetSalesProfileForMobile(int id)
        {
            var param = Db.SystemParameters.FirstOrDefault(x => x.Code.Equals("DEF_SALES_TAX_INC"))?.Value;

            var data = (from user in Db.Users
                join empl in Db.Employees on user.EmployeeId equals empl.Id
                join groupSl in Db.SalesmanGroups on empl.SalesGroupId equals groupSl.Id
                select new SalesProfile
                {
                    UserId = user.Id,
                    SalesName = empl.FirstName + " " + empl.LastName,
                    SalesId = empl.Id,
                    SalesInitialId = empl.Initial,
                    SalesGroup = groupSl.Name,
                    TaxInclude = int.Parse(param)
                }).SingleOrDefault(x => x.UserId.Equals(id));
            return data;
        }

        public SaveResult InsertTracking(SalesTracking data, int userId)
        {
            var result = new SaveResult(false);
            try
            {
                var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
                Db.SalesmanMapTrackingHistories.Add(new SalesmanMapTrackingHistory
                {
                    SalesmanId= (long)salesId,
                    Lat=data.Lat,
                    Lng=data.Lng,
                    TrackedDate=data.TrackedDate
                });

                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data;
            result.Message = "Sales Tracking berhasil disimpan.";
            return result;
        }
        #endregion
    }
}
