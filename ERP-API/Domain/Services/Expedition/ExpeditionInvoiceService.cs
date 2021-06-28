using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Expedition;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Expedition;
using ERP_API.Domain.Models;
using ERP_API.Model.Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Services.Expedition
{
    public class ExpeditionInvoiceService : GeneralService<ExpeditionInvoiceHeader>, IExpeditionInvoiceService
    {
        public ExpeditionInvoiceService(TenantContext db)
            :base(db)
        {

        }
       
        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.ExpeditionInvoiceHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.RefNo.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ExpeditionInvoiceDetail> GetDetailData(string code)
        {
            return Db.ExpeditionInvoiceDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(ExpeditionInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("EI_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.ExpeditionInvoiceHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.ExpeditionInvoiceDetails.Add(new ExpeditionInvoiceDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        TransCode = item.TransCode
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
            result.Message = "Data faktur ekspedisi berhasil disimpan.";
            return result;
        }

        public SaveResult Update(ExpeditionInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.ExpeditionInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data faktur ekspedisi tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                Db.ExpeditionInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in invoice before
                var delDetails = Db.ExpeditionInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in invoice before
                Db.ExpeditionInvoiceDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        Db.ExpeditionInvoiceDetails.Add(new ExpeditionInvoiceDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            TransCode = item.TransCode
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.ExpeditionInvoiceDetails.Update(item);
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
            result.Message = "Data faktur ekspedisi berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.ExpeditionInvoiceHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data faktur ekspedisi tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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
            result.Message = "Data faktur ekspedisi berhasil ditandai sebagai void.";
            return result;
        }
    }
}
