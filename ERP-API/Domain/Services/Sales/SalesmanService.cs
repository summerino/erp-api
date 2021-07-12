using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;

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
            else
            {
                return Db.VwSalesmanSchedules.Where(x => x.Id == 0);
            }
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
    }
}
