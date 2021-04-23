using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model.General;

namespace ERP_API.Domain.Services.General
{
    public class CurrencyService : GeneralService<Currency>, ICurrencyService
    {
        public CurrencyService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.Currencies.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Currencies.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public Currency FindByCode(string code)
        {
            return Db.Currencies.Find(code);
        }

        public SaveResult Insert(CurrencyRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsCodeExists(data.Code))
                {
                    result.Message = "Code is already exists. Please use another code.";
                    return result;
                }

                //Place at the bottom of sort if sort = 0
                var isSortDefault = false;
                if (data.Sort == 0)
                {
                    var maxSort = Db.Currencies.OrderByDescending(x => x.Sort).First().Sort;
                    data.Sort = maxSort + 1;

                    isSortDefault = true;
                }

                Db.Add(data);

                Db.SaveChanges();

                if (!isSortDefault)
                {
                    // Execute sp_update_currency_sort
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_currency_sort {0}", data.SortValue);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success insert currency.";
            return result;
        }

        public SaveResult Update(CurrencyRequest data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.Currencies.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            if (data.Sort != 0)
            {
                // Execute sp_update_currency_sort
                Db.Database.ExecuteSqlRaw("EXEC sp_update_currency_sort {0}", data.SortValue);
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update currency.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Currencies.Find(code);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Can't inactive Currency because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive currency.";
            return result;
        }

        private bool IsCodeExists(string code)
        {
            return Db.Currencies.Any(x => x.Code == code);
        }
    }
}
