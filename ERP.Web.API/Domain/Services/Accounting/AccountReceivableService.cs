using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Entity;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class AccountReceivableService : GeneralService<BeginningBalanceAR>, IAccountReceivableService
    {
        public AccountReceivableService(TenantContext db):
            base(db)
        {
                
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwBeginningBalanceARs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.CustName.Contains(search) || x.CurrCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(BeginningBalanceAR data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking code already exists or not
                if (IsCodeExists(data.Code, 0))
                {
                    result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                    return result;
                }

                Db.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data piutang berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(BeginningBalanceAR data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCodeExists(data.Code, data.Id))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }

            // Update data
            Db.BeginningBalanceARs.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data piutang berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            throw new NotImplementedException();
        }

        private bool IsCodeExists(string code, long id)
        {
            return Db.BeginningBalanceARs.Any(x => x.Code == code && x.Id != id);
        }
    }
}
