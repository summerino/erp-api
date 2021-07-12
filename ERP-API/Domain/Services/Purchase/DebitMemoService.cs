using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Purchase
{
    public class DebitMemoService : GeneralService<DebitMemo>, IDebitMemoService
    {
        public DebitMemoService(TenantContext db)
            : base(db)
        {
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.DebitMemos.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data nota debit tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data nota debit berhasil ditandai sebagai void.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwDebitMemos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.SrcTransName == search ||
                        x.TransCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var piD = from dt in Db.PurchaseInvoiceDetails
                      where dt.RcvCode == code
                      select dt.Code;

            var data = from piH in Db.PurchaseInvoiceHeaders
                       where piD.Contains(piH.Code) && piH.Mark == "A"
                       select new { piH.Code, piH.Date, piH.Total };

            return data.ToDynamicList();
        }

        public override SaveResult Insert(DebitMemo data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {

                // Get new code
                var newCode = GetNewCode("DM_NUM_FMT", data.CreatedDate);

                // Insert data
                data.Code = newCode;
                // Insert data
                Db.DebitMemos.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data nota debit berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(DebitMemo data)
        {
            var result = new SaveResult(false);           

            // Update data
            Db.DebitMemos.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            
            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data nota debit berhasil diperbarui.";
            return result;
        }


    }
}
