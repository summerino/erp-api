using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting;

public class BeginningBalanceAccountPayableService : GeneralService<BeginningBalanceAP>, IBeginningBalanceAccountPayableService
{
    public BeginningBalanceAccountPayableService(TenantContext db):
        base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search)
    {
        var data = Db.VwBeginningBalanceAPs.Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x => x.Code.Contains(search) || x.SupName.Contains(search));
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
        result.Message = "Data saldo awal hutang berhasil disimpan.";
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

        if (data.PaidAmount > data.Amount)
        {
            result.Message = "Nilai tidak boleh lebih kecil dari Nilai yang terbayarkan.";
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
        result.Message = "Data saldo awal hutang berhasil diperbarui.";
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
                    result.Message = "Tidak bisa menghapus data saldo awal hutang karena telah digunakan pada data bank tunai.";
                    return result;
                }
            }

            // Delete data
            Db.BeginningBalanceAPs.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data saldo awal hutang berhasil dihapus.";
        return result;
    }

    public IEnumerable<UploadBBAPRequest> VerifyUpload(IEnumerable<UploadBBAPRequest> data)
    {
        foreach (var item in data)
        {
            var result = Db.BeginningBalanceAPs.FirstOrDefault(x => x.Code == item.Kode);
            if (result != null)
            {
                var sup = Db.Suppliers.FirstOrDefault(x => x.Code == item.Kodepemasok);
                var startDate = Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE");
                var isDuplicate = data.Where(x => x.Kode == item.Kode).GroupBy(x => x.Kode).Any(g => g.Count() > 1);

                if (((item.Nilai - result.PaidAmount) < 0) || 
                    (sup == null) || 
                    (item.Tanggal > Convert.ToDateTime(startDate.Value)) ||
                    isDuplicate)
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
                var sup = Db.Suppliers.FirstOrDefault(x => x.Code == item.Kodepemasok);
                var startDate = Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE");
                var isDuplicate = data.Where(x => x.Kode == item.Kode).GroupBy(x => x.Kode).Any(g => g.Count() > 1);

                if (item.Kode == null ||
                    (sup == null) ||
                    (item.Tanggal > Convert.ToDateTime(startDate.Value)) ||
                    isDuplicate)
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
    public SaveResult Posting(IEnumerable<UploadBBAPRequest> data, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var verified = VerifyUpload(data).ToList();
            if(!verified.Where(x => !x.Mark).Any())
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

            //var removed = Db.BeginningBalanceAPs
            //    .Where(x => !verified.Select(y => y.Kode).Contains(x.Code))
            //    .ToList();

            //if (removed.Any())
            //    Db.BeginningBalanceAPs.RemoveRange(removed);

            foreach (var item in verified)
            {
                if (!item.Mark)
                {
                    var dataAp = Db.BeginningBalanceAPs.FirstOrDefault(x => x.Code == item.Kode);
                    if (dataAp != null)
                    {
                        dataAp.Date = item.Tanggal ?? DateTime.MinValue;
                        dataAp.DueDate = item.Tgljatuhtempo ?? DateTime.MaxValue;
                        dataAp.SupCode = item.Kodepemasok.ToUpper();
                        dataAp.Amount = item.Nilai;
                        dataAp.Notes = item.Catatan;
                        dataAp.IsActive = true;
                        dataAp.UpdatedBy = userId;
                        dataAp.UpdatedDate = DateTime.Now;

                        Db.BeginningBalanceAPs.Update(dataAp);
                    }
                    else
                    {
                        Db.BeginningBalanceAPs.Add(new BeginningBalanceAP
                        {
                            Code = item.Kode,
                            Date = item.Tanggal ?? DateTime.MinValue,
                            DueDate = item.Tgljatuhtempo ?? DateTime.MaxValue,
                            SupCode = item.Kodepemasok.ToUpper(),
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
        result.Message = "Import data saldo awal hutang berhasil disimpan.";
        return result;
    }

    private bool IsCodeExists(string code, long id)
    {
        return Db.BeginningBalanceAPs.Any(x => x.Code == code && x.Id != id);
    }

}