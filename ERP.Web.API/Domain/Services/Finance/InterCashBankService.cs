using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Services.Finance
{
    public class InterCashBankService : GeneralService<GeneralCashBankHeader>, IInterCashBankService
    {
        public InterCashBankService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwInterCashBankHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.Type.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code)
        {
            var data = Db.VwGeneralCashBankDetails.Where(x => x.Code.Equals(code));
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
                var newCode2 = GetNewCode("CB_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                data.IsInterCashBank = true;
                Db.GeneralCashBankHeaders.Add(data);

                if (data.ItemDetails.Any())
                {
                    var orderedData = data.ItemDetails.OrderBy(x => x.LineNo);
                    short j = 0;
                    string coaCode = "";
                    string currCode = "";
                    decimal rate = 0;
                    decimal amount = 0;
                    foreach (var item in orderedData)
                    {
                        // Save Credit for data
                        if (j == 1)
                        {
                            Db.GeneralCashBankHeaders.Add(new GeneralCashBankHeader
                            {
                                Code = newCode2,
                                VouCode = data.VouCode,
                                Type = "D",
                                Date = data.Date,
                                CoaCode = coaCode,
                                CurrCode = currCode,
                                Rate = rate,
                                Amount = amount,
                                ChequeNo = data.ChequeNo,
                                ChequeDate = data.ChequeDate,
                                Notes = data.Notes,
                                IsInterCashBank = true,
                                Mark = data.Mark,
                                CreatedBy = data.CreatedBy,
                                CreatedDate = data.CreatedDate,
                                UpdatedBy = data.UpdatedBy,
                                UpdatedDate = data.UpdatedDate
                            });
                        }

                        Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                        {
                            Code = (j == 1) ? newCode2 : data.Code,
                            LineNo = j,
                            Type = item.Type,
                            TransCode = (j == 1) ? data.Code : newCode2,
                            CoaCode = item.CoaCode,
                            CurrCode = item.CurrCode,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            TypeAmount = item.TypeAmount,
                            TransAmount = item.TransAmount,
                            Notes = item.Notes
                        });

                        if (j == 0)
                        {
                            coaCode = item.CoaCode;
                            currCode = item.CurrCode;
                            rate = item.Rate;
                            amount = item.Amount;
                        }
                        j++;
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
            result.Message = "Data pemindahan dana berhasil disimpan.";
            return result;
        }

        public SaveResult Update(CashBankRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.GeneralCashBankHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data pemindahan dana tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.GeneralCashBankHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                if (data.ItemDetails.Any())
                {
                    var orderedData = data.ItemDetails.OrderBy(x => x.LineNo);
                    short j = 0;
                    foreach (var item in orderedData)
                    {
                        // Load secondary transaction
                        var data2 = Db.GeneralCashBankHeaders.Find(item.TransCode);

                        // Save Credit for data
                        if (j == 0)
                        {
                            data2.Date = data.Date;
                            data2.Amount = data.Amount;
                            data2.ChequeNo = data.ChequeNo;
                            data2.ChequeDate = data.ChequeDate;
                            data2.Notes = data.Notes;
                            Db.GeneralCashBankHeaders.Update(data2);
                            Db.Entry(data2).Property(e => e.Code).IsModified = false;
                            Db.Entry(data2).Property(e => e.CreatedBy).IsModified = false;
                            Db.Entry(data2).Property(e => e.CreatedDate).IsModified = false;
                        }

                        var dataDetail = Db.GeneralCashBankDetails.Where(x => x.Code == item.Code).FirstOrDefault();
                        dataDetail.Rate = item.Rate;
                        dataDetail.Amount = item.Amount;
                        dataDetail.TransAmount = item.TransAmount;
                        dataDetail.Notes = item.Notes;
                        Db.Entry(dataDetail).Property(e => e.Code).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.LineNo).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.TransCode).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.Type).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.CoaCode).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.CurrCode).IsModified = false;
                        Db.Entry(dataDetail).Property(e => e.TypeAmount).IsModified = false;
                        j++;
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
            result.Message = "Data pemindahan dana berhasil diperbarui.";
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
                    result.Message = "Data pemindahan dana tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data pemindahan dana berhasil ditandai sebagai void.";
            return result;
        }


    }
}
