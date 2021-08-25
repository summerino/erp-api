using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Web.API.Domain.Services.HumanResources
{
    public class AttendanceService : IAttendanceService
    {
        private readonly TenantContext _db;
        public AttendanceService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetDataReport(string startDate, string endDate, short employeeType, int salesGroupId, string employee, IEnumerable<Sort> sorts)
        {
            var query = _db.VwAttendanceReports.AsQueryable();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                query = query.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                query = query.Where(x => x.Date >= Convert.ToDateTime(startDate)).AsQueryable();
            }

            if(!string.IsNullOrWhiteSpace(employee))
                query = query.Where(x => x.Name.Contains(employee));

            if (employeeType > 0)
                query = query.Where(x => x.Type.Equals(employeeType));

            if (salesGroupId > 0)
                query = query.Where(x => x.SalesGroupId.Equals(salesGroupId));

            return query.AsQueryable().ToDataSourceResult(0, query.Count(), null, sorts);
        }
    }
}
