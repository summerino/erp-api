using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Services.Accounting
{
    public class AccountPayableService : GeneralService<BeginningBalanceAP>, IAccountPayableService
    {
        public AccountPayableService(TenantContext db):
            base(db)
        {
                
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwBeginningBalanceAPs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.CurrCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(BeginningBalanceAP data)
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

        public override SaveResult Update(BeginningBalanceAP data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCodeExists(data.Code, data.Id))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }

            // Update data
            Db.BeginningBalanceAPs.Update(data);
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
            return Db.BeginningBalanceAPs.Any(x => x.Code == code && x.Id != id);
        }
    }
}
