using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Entity;
using ERP.Entity.General;
using Microsoft.EntityFrameworkCore;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Services.General
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
                    result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
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
            result.Message = "Data mata uang berhasil disimpan.";
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
            result.Message = "Data mata uang berhasil diperbarui.";
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
                    result.Message = "Tidak bisa menonaktifkan data mata uang karena data sudah nonaktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data mata uang berhasil dinonaktifkan.";
            return result;
        }

        private bool IsCodeExists(string code)
        {
            return Db.Currencies.Any(x => x.Code == code);
        }
    }
}
