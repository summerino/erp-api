using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class BeginningBalanceDebitMemoService : GeneralService<BeginningBalanceDebitMemo>, IBeginningBalanceDebitMemoService
    {
        public BeginningBalanceDebitMemoService(TenantContext db)
            :base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search)
        {
            var data = Db.VwBeginningBalanceDebitMemos.Where(x => x.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(BeginningBalanceDebitMemo data)
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
            result.Message = "Data saldo awal nota debit berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(BeginningBalanceDebitMemo data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCodeExists(data.Code, data.Id))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }
            
            // Update data
            Db.BeginningBalanceDebitMemos.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data saldo awal nota debit berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(long id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.BeginningBalanceDebitMemos.Find(id);
            if (data != null)
            {
                //Check if any cash bank already using this BBDM
                var validRes = Db.GeneralCashBankDetails
                            .Join(Db.GeneralCashBankHeaders, detail => detail.Code, header => header.Code, (detail, header) => new { Detail = detail, Header = header })
                            .Where(x => x.Detail.TransCode == data.Code).ToList();
                foreach (var item in validRes)
                {
                    if (item.Header.Mark == "A")
                    {
                        result.Message = "Tidak bisa menghapus data saldo awal nota debit karena telah digunakan pada data bank tunai.";
                        return result;
                    }
                }

                // Delete data
                Db.BeginningBalanceDebitMemos.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data saldo awal nota debit berhasil dihapus.";
            return result;
        }

        private bool IsCodeExists(string code, long id)
        {
            return Db.BeginningBalanceDebitMemos.Any(x => x.Code == code && x.Id != id);
        }
    }
}
