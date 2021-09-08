using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class CreditMemoService : GeneralService<CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwCreditMemos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustName.Contains(search) || x.SrcTransName == search ||
                        x.TransCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
        public DataSourceResult GetDataOutstandingCreditMemo(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwOutstandingCreditMemos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
        public List<dynamic> GetRelatedTransactions(string code)
        {
            //var piD = from dt in Db.SalesInvoiceDetails
            //          where dt.DoCode == code
            //          select dt.Code;

            //var data = from piH in Db.SalesInvoiceHeaders
            //           where piD.Contains(piH.Code) && piH.Mark == "A"
            //           select new { piH.Code, piH.Date, piH.Total };
            
            var data = (from h in Db.GeneralCashBankHeaders
                        join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                        where h.Mark == "A" && d.TransCode == code
                        select new
                        {
                            h.Code,
                            h.Date,
                            Total = h.Amount
                        });
            return data.ToDynamicList();
        }

        public override SaveResult Insert(CreditMemo data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {

                // Get new code
                var newCode = GetNewCode("CM_NUM_FMT", data.CreatedDate);

                // Insert data
                data.Code = newCode;
                Db.CreditMemos.Add(data);

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
            result.Message = "Data nota kredit berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(CreditMemo data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.CreditMemos.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.SrcTrans).IsModified = false;
            Db.Entry(data).Property(e => e.Used).IsModified = false;
            Db.Entry(data).Property(e => e.Mark).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data nota kredit berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.CreditMemos.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data nota kredit tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data nota kredit berhasil ditandai sebagai void.";
            return result;
        }
    }
}
