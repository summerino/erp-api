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
                        x.Code.Contains(search) || x.CoaCode.Contains(search) || x.CoaNameFrom.Contains(search) ||
                        x.CoaCodeTo.Contains(search) || x.CoaNameTo.Contains(search));
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
                var crossCoa = Db.SystemParameters.FirstOrDefault(x => x.Code == "CROSS_COA" && x.IsActive)?.Value;

                if (string.IsNullOrWhiteSpace(crossCoa))
                {
                    result.Message = "Data pemindahan dana tidak bisa disimpan karena akun ayat silang belum diatur di pengaturan sistem.";
                    return result;
                }

                // Insert header data
                data.Code = newCode;
                data.IsInterCashBank = true;
                Db.GeneralCashBankHeaders.Add(data);

                if (data.ItemDetails.Any())
                {
                    foreach (var item in data.ItemDetails.OrderBy(x => x.LineNo))
                    {
                        // Save credit for data
                        if (item.Type.ToUpper() == "ICBI")
                        {
                            Db.GeneralCashBankHeaders.Add(new GeneralCashBankHeader
                            {
                                Code = newCode2,
                                VouCode = data.VouCode,
                                Type = "D",
                                Date = data.Date,
                                CoaCode = item.CoaCode,
                                CurrCode = item.CurrCode,
                                Rate = data.Rate,
                                Amount = data.Amount,
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
                            Code = item.Type.ToUpper() == "ICBI" ? newCode2 : data.Code,
                            LineNo = 1,
                            Type = item.Type,
                            TransCode = item.Type.ToUpper() == "ICBI" ? data.Code : newCode2,
                            CoaCode = crossCoa,
                            CurrCode = item.CurrCode,
                            Rate = item.Rate,
                            Amount = item.Amount,
                            TypeAmount = item.TypeAmount,
                            TransAmount = item.Amount,
                            Notes = item.Notes,
                            Src = "ICB"
                        });
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

                // Get cross coa
                var crossCoa = Db.SystemParameters.FirstOrDefault(x => x.Code == "CROSS_COA" && x.IsActive)?.Value;

                if (string.IsNullOrWhiteSpace(crossCoa))
                {
                    result.Message = "Data pemindahan dana tidak bisa diubah karena akun ayat silang belum diatur di pengaturan sistem.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.Set<GeneralCashBankHeader>().Attach(data);
                Db.Entry(data).Property(e => e.Date).IsModified = true;
                Db.Entry(data).Property(e => e.Amount).IsModified = true;
                Db.Entry(data).Property(e => e.ChequeNo).IsModified = true;
                Db.Entry(data).Property(e => e.ChequeDate).IsModified = true;
                Db.Entry(data).Property(e => e.Notes).IsModified = true;
                Db.Entry(data).Property(e => e.UpdatedBy).IsModified = true;
                Db.Entry(data).Property(e => e.UpdatedDate).IsModified = true;
                Db.Entry(data).Property(e => e.ApprovedBy).IsModified = true;
                Db.Entry(data).Property(e => e.ApprovedDate).IsModified = true;

                if (data.ItemDetails.Any())
                {
                    short j = 1;
                    foreach (var item in data.ItemDetails.OrderBy(x => x.LineNo))
                    {
                        // Load secondary transaction
                        var data2 = Db.GeneralCashBankHeaders.Find(item.TransCode);

                        // Save credit for data
                        if (j == 1)
                        {
                            data2.Date = data.Date;
                            data2.Amount = data.Amount;
                            data2.ChequeNo = data.ChequeNo;
                            data2.ChequeDate = data.ChequeDate;
                            data2.Notes = data.Notes;
                            data2.UpdatedBy = data.UpdatedBy;
                            data2.UpdatedDate = data.UpdatedDate;
                            data2.ApprovedBy = null;
                            data2.ApprovedDate = null;
                        }

                        var dataDetail = Db.GeneralCashBankDetails.FirstOrDefault(x => x.Code == item.Code);
                        dataDetail.CoaCode = crossCoa;
                        dataDetail.Rate = item.Rate;
                        dataDetail.Amount = item.Amount;
                        dataDetail.TransAmount = item.Amount;
                        dataDetail.Notes = item.Notes;
                        
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

        public SaveResult Delete(string code, string transCode, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.GeneralCashBankHeaders.Where(x => x.Code == code || x.Code == transCode);
            if (data.Any())
            {
                // Checking mark header data
                if (data.Any(x => x.Mark == "V"))
                {
                    result.Message = "Data pemindahan dana tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                foreach (var item in data)
                {
                    item.Mark = "V";
                    item.UpdatedBy = userId;
                    item.UpdatedDate = DateTime.Now;
                }

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data pemindahan dana berhasil ditandai sebagai void.";
            return result;
        }
    }
}
