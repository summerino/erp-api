using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class BeginningBalanceCreditMemoService : GeneralService<BeginningBalanceCreditMemo>, IBeginningBalanceCreditMemoService
    {
        public BeginningBalanceCreditMemoService(TenantContext db)
            :base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search)
        {
            var data = Db.VwBeginningBalanceCreditMemos.Where(x => x.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustCode.StartsWith(search) || x.CustName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(BeginningBalanceCreditMemo data)
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
            result.Message = "Data saldo awal nota kredit berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(BeginningBalanceCreditMemo data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCodeExists(data.Code, data.Id))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }

            // Update data
            Db.BeginningBalanceCreditMemos.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data saldo awal nota kredit berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(long id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.BeginningBalanceCreditMemos.Find(id);
            if (data != null)
            {

                //Check if any cash bank already using this BBCM
                var validRes = Db.GeneralCashBankDetails
                            .Join(Db.GeneralCashBankHeaders, detail => detail.Code, header => header.Code, (detail, header) => new { Detail = detail, Header = header })
                            .Where(x => x.Detail.TransCode == data.Code).ToList();
                foreach (var item in validRes)
                {
                    if (item.Header.Mark == "A")
                    {
                        result.Message = "Tidak bisa menghapus data saldo awal nota kredit karena telah digunakan pada data bank tunai.";
                        return result;
                    }
                }

                // Delete data
                Db.BeginningBalanceCreditMemos.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data saldo awal nota kredit berhasil dihapus.";
            return result;
        }

        public IEnumerable<UploadBBCMRequest> VerifyUpload(IEnumerable<UploadBBCMRequest> data)
        {
            foreach (var item in data)
            {
                var result = Db.BeginningBalanceCreditMemos.FirstOrDefault(x => x.Code == item.Kode);
                if (result != null)
                {
                    var cust = Db.Customers.FirstOrDefault(x => x.Code == item.Kodepelanggan);
                    var startDate = Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE");
                    var isDuplicate = data.Where(x => x.Kode == item.Kode).GroupBy(x => x.Kode).Any(g => g.Count() > 1);

                    if (
                        ((item.Nilai - result.Used) < 0) ||
                        (cust == null) ||
                        string.IsNullOrEmpty(item.Tipe) ||
                        (item.Tanggal > Convert.ToDateTime(startDate.Value)) ||
                        isDuplicate
                    )
                    {
                        item.Mark = true;
                    }
                    else
                    {
                        item.Mark = false;
                    }
                }
                else
                {
                    if (item.Kode == null)
                    {
                        item.Mark = true;
                    }
                    else
                    {
                        item.Mark = false;
                    }
                }
            }
            return data;
        }

        public SaveResult Posting(IEnumerable<UploadBBCMRequest> data, int userId)
        {
            var result = new SaveResult(false);
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var verified = VerifyUpload(data).ToList();
                if (!verified.Where(x => !x.Mark).Any())
                {
                    result.Message = "Tidak ada data yang dapat diproses.";
                    return result;
                }
                var verifyDupe = verified.GroupBy(x => x.Kode).Any(g => g.Count() > 1);
                if (verifyDupe)
                {
                    result.Message = "Terdapat kode duplikat pada data import.";
                    return result;
                }

                //var removed = Db.BeginningBalanceCreditMemos
                //    .Where(x => !verified.Select(y => y.Kode).Contains(x.Code))
                //    .ToList();
                //if (removed.Any())
                //    Db.BeginningBalanceCreditMemos.RemoveRange(removed);

                foreach (var item in verified)
                {
                    if (!item.Mark)
                    {
                        var dataCm = Db.BeginningBalanceCreditMemos.FirstOrDefault(x => x.Code == item.Kode);
                        if (dataCm != null)
                        {
                            dataCm.Date = item.Tanggal ?? DateTime.MinValue;
                            dataCm.Type = (short)(item.Tipe.ToLower() == "deposit" ? 1 : item.Tipe.ToLower() == "retur" ? 2 : 0);
                            dataCm.CustCode = item.Kodepelanggan.ToUpper();
                            dataCm.Amount = item.Nilai;
                            dataCm.Notes = item.Catatan;
                            dataCm.IsActive = true;
                            dataCm.UpdatedBy = userId;
                            dataCm.UpdatedDate = DateTime.Now;
                            Db.BeginningBalanceCreditMemos.Update(dataCm);
                        }
                        else
                        {
                            Db.BeginningBalanceCreditMemos.Add(new BeginningBalanceCreditMemo
                            {
                                Code = item.Kode,
                                Date = item.Tanggal ?? DateTime.MinValue,
                                Type = (short)(item.Tipe.ToLower() == "deposit" ?  1 : item.Tipe.ToLower() == "retur" ? 2 : 0),
                                CustCode = item.Kodepelanggan.ToUpper(),
                                CurrCode = "IDR",
                                Rate = 1,
                                Amount = item.Nilai,
                                Notes = item.Catatan,
                                IsActive = true,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                                UpdatedBy = userId,
                                UpdatedDate = DateTime.Now,
                            });
                        }
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
            result.Message = "Import data saldo awal nota kredit berhasil disimpan.";
            return result;
        }

        private bool IsCodeExists(string code, long id)
        {
            return Db.BeginningBalanceCreditMemos.Any(x => x.Code == code && x.Id != id);
        }
    }
}
