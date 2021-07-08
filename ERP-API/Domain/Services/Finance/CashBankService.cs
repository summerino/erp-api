using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Finance;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Finance;
using ERP_API.Domain.Models;
using ERP_API.Model.Finance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP_API.Domain.Services.Finance
{
    public class CashBankService : GeneralService<CashBankRequest>, ICashBankService
    {
        public CashBankService(TenantContext db)
            : base(db)
        {
        }
        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.GeneralCashBankHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.VouCode.Contains(search) || x.Type.Contains(search) ||
                        x.CurrCode == search || x.ChequeNo.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }
        public IEnumerable<GeneralCashBankDetail> GetDetailData(string code)
        {
            var data = Db.GeneralCashBankDetails.Where(x => x.Code.Equals(code));
            return data;
        }
        public SaveResult Insert(CashBankRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("CB_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.GeneralCashBankHeaders.Add(data);

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                    {
                        Code = data.Code,
                        LineNo = j,
                        Type = item.Type,
                        TransCode = item.TransCode,
                        CoaCode = item.CoaCode,
                        CurrCode = item.CurrCode,
                        Rate = item.Rate,
                        Amount = item.Amount,
                        TypeAmount = item.TypeAmount,
                        TransAmount = item.TransAmount,
                        Notes = item.Notes
                    });
                    j++;
                }


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
            result.Message = "Data bank tunai berhasil disimpan.";
            return result;
        }

        public SaveResult Update(CashBankRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.GeneralCashBankHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data bank tunai tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.GeneralCashBankHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.GeneralCashBankDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.GeneralCashBankDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {

                        Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            Type = item.Type,
                            TransCode = item.TransCode,
                            CoaCode = item.CoaCode,
                            CurrCode = item.CurrCode,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            TypeAmount = item.TypeAmount,
                            TransAmount = item.TransAmount,
                            Notes = item.Notes
                        });

                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.GeneralCashBankDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                    }
                }

                
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
            result.Message = "Data bank tunai berhasil diperbarui.";
            return result;
        }
        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.GeneralCashBankHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data bank tunai tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data bank tunai berhasil ditandai sebagai void.";
            return result;
        }

        
    }
}
