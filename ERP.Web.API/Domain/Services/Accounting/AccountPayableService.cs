using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
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

                // Check Date greater than Data Start Date
                if (data.Date >= Convert.ToDateTime(Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value))
                {
                    result.Message = "Tanggal tidak boleh lebih dari tanggal mulai data.";
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

            // Check Date greater than Data Start Date
            if (data.Date >= Convert.ToDateTime(Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value))
            {
                result.Message = "Tanggal tidak boleh lebih dari tanggal mulai data.";
                return result;
            }

            // Update data
            Db.BeginningBalanceAPs.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.PaidAmount).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data piutang berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(long id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.BeginningBalanceAPs.Find(id);
            if (data != null)
            {

                //Check if any cash bank already using this BBAP
                var validRes = Db.GeneralCashBankDetails
                            .Join(Db.GeneralCashBankHeaders, detail => detail.Code, header => header.Code, (detail, header) => new { Detail = detail, Header = header })
                            .Where(x => x.Detail.TransCode == data.Code).ToList();
                foreach (var item in validRes)
                {
                    if (item.Header.Mark == "A")
                    {
                        result.Message = "Tidak bisa menghapus data hutang karena telah digunakan pada data bank tunai.";
                        return result;
                    }
                }

                // Delete data
                Db.BeginningBalanceAPs.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data hutang berhasil dihapus.";
            return result;
        }

        private bool IsCodeExists(string code, long id)
        {
            return Db.BeginningBalanceAPs.Any(x => x.Code == code && x.Id != id);
        }
    }
}
