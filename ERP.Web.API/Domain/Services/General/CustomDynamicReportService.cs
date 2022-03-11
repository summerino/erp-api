using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services
{
	public class CustomDynamicReportService : ICustomDynamicReportService
	{
		private readonly TenantContext _db;
		public CustomDynamicReportService(TenantContext db)
		{
			_db = db;
		}

        public dynamic GetDataReport(int? id, string param1, string param2, string param3, string param4, string param5)
        {
			var result = new SaveResult(false);
			var generalQuery = new GeneralQuery();
            if (id.HasValue)
            {
				var tData = _db.DynamicReportTemplates.FirstOrDefault(x => x.Id == id);
				result = generalQuery.DynamicQuery(_db.Database.GetConnectionString(), tData.Query, param1,
					param2, param3, param4, param5);
			}

			return result;
		}

        public dynamic GetDataSource(string source)
        {
			var generalQuery = new GeneralQuery();
			var result = generalQuery.DynamicQuery(_db.Database.GetConnectionString(), source: source);

			return result;
		}
	}
}
