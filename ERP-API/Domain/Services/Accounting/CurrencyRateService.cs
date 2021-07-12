using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Domain.Models;
using ERP_API.Model.Accounting;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domain.Services.Accounting
{
    public class CurrencyRateService : GeneralService<CurrencyRate>, ICurrencyRateService
    {
        public CurrencyRateService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.CurrencyRates.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate) 
                    ? data.Where(x => x.Date == searchDate) : 
                    data.Where(x => x.CurrCode.Contains(search) || x.Amount.ToString().Contains(search));
            }
            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.CurrencyRates;
            return data.ToDataSourceResult(0, -1, filters, sorts);
        }
        
        public override SaveResult Update(CurrencyRate data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.CurrencyRates.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data nilai tukar mata uang berhasil diperbarui.";
            return result;
        }

        public SaveResult Insert(CurrencyRateRequest data)
        {
            var result = new SaveResult(false);

            try
            {
                // Insert data
                Db.Database.ExecuteSqlInterpolated($"EXEC sp_insert_curr_rate {data.StartDate},{data.EndDate},{data.CurrCode},{data.Amount},{data.CreatedBy}");
                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Data nilai tukar mata uang berhasil disimpan.";
            return result;
        }
    }
}
