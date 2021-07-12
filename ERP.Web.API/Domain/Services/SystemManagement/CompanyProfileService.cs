using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;

namespace ERP.Web.API.Domain.Services.SystemManagement
{
    public class CompanyProfileService : ICompanyProfileService
    {
        private readonly TenantContext _tenantCtx;

        public CompanyProfileService(TenantContext tenantContext)
        {
            _tenantCtx = tenantContext;
        }
        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = _tenantCtx.VwCompanies.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                         x.Name.Contains(search) || x.Address1.Contains(search) || x.Address2.Contains(search)
                         || x.City.Contains(search) || x.Zip.Contains(search) || x.Phone.Contains(search)
                         || x.Fax.Contains(search) || x.Npwp.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult Update(Company data)
        {
            var result = new SaveResult(false);

            using var transaction = _tenantCtx.Database.BeginTransaction();
            try
            {
                // Update data
                _tenantCtx.Companies.Update(data);
                _tenantCtx.Entry(data).Property(e => e.Id).IsModified = false;
                _tenantCtx.Entry(data).Property(e => e.Name).IsModified = false;
                _tenantCtx.Entry(data).Property(e => e.CatalogTenantId).IsModified = false;


                _tenantCtx.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Name;
            result.Message = "Data profil perusahaan diperbarui.";
            return result;
        }
    }
}
