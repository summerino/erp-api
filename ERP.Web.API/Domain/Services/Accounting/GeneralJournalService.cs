using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Accounting;
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
    public class GeneralJournalService : GeneralService<GeneralJournalHeader>, IGeneralJournalService
    {
        public GeneralJournalService(TenantContext db)
            :base(db)
        {

        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwGeneralJournalHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CurrCode.Contains(search) || x.Total.ToString().Contains(search)
                        || x.Notes.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<GeneralJournalDetail> GetDetailData(string code)
        {
            return Db.GeneralJournalDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(GeneralJournalRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Verified Total Debit & Credit
                if (data.TotalDebit != data.TotalCredit)
                {
                    result.Message = "Nominal total debit tidak sama dengan nomimal total kredit.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("GEN_JR_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.GeneralJournalHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.GeneralJournalDetails.Add(new GeneralJournalDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        CoaCode = item.CoaCode,
                        Notes = item.Notes,
                        Type = item.CreditValue != 0 ? "C" : "D",
                        Amount = (decimal)(item.CreditValue != 0 ? item.CreditValue : item.DebitValue)
                    });
                }

                // Save changes
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
            result.Message = "Data jurnal umum berhasil disimpan.";
            return result;
        }

        public SaveResult Update(GeneralJournalRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.GeneralJournalHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data jurnal umum tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                // Verified Total Debit & Credit
                if (data.TotalDebit != data.TotalCredit)
                {
                    result.Message = "Nominal total debit tidak sama dengan nomimal total kredit.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.GeneralJournalHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in invoice before
                var delDetails = Db.GeneralJournalDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in invoice before
                Db.GeneralJournalDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        Db.GeneralJournalDetails.Add(new GeneralJournalDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            CoaCode = item.CoaCode,
                            Notes = item.Notes,
                            Type = item.CreditValue != 0 ? "C" : "D",
                            Amount = (decimal)(item.CreditValue != 0 ? item.CreditValue : item.DebitValue)
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;
                        item.Type = item.Type;
                        item.Amount = item.Type == "D" ? (decimal)item.DebitValue : (decimal)item.CreditValue;

                        Db.GeneralJournalDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Save changes
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
            result.Message = "Data jurnal umum berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.GeneralJournalHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data jurnal umum tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                using var transaction = Db.Database.BeginTransaction();
                try
                {
                    // Update header data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    // Save changes
                    Db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data jurnal umum berhasil ditandai sebagai void.";
            return result;
        }
    }
}
