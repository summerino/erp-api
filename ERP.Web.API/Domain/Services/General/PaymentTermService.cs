using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Domain.Services.General;

public class PaymentTermService : GeneralService<PaymentTerm>, IPaymentTermService
{
    public PaymentTermService(TenantContext db)
        :base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwPaymentTerms.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Initial.Contains(search) || x.Name.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.VwPaymentTerms.Where(x => x.IsActive);

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public override SaveResult Insert(PaymentTerm data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, 0))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
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
        result.Message = "Data syarat pembayaran berhasil disimpan.";
        return result;
    }

    public override SaveResult Update(PaymentTerm data)
    {
        var result = new SaveResult(false);

        // Checking initial already exists or not
        if (IsInitialExists(data.Initial, data.Id))
        {
            result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
            return result;
        }

        //Check if payment term is cash
        if (data.Id == 1)
        {
            result.Message = "Syarat pembayaran cash tidak dapat diubah.";
            return result;
        }

        // Update data
        Db.PaymentTerms.Update(data);
        Db.Entry(data).Property(e => e.Id).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Id;
        result.Message = "Data syarat pembayaran berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(int id, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.PaymentTerms.Find(id);
        if (data != null)
        {
            // Checking active
            if (data.IsActive == false)
            {
                result.Message = "Tidak bisa menghapus data syarat pembayaran karena data sudah dihapus.";
                return result;
            }

            //Check if any customers already using this type
            if (Db.Customers.Any(x => x.PaymentTermId == data.Id))
            {
                result.Message = "Tidak bisa menghapus data syarat pembayaran karena telah digunakan pada data pelanggan.";
                return result;
            }

            //Check if payment term is cash
            if (data.Id == 1)
            {
                result.Message = "Syarat pembayaran cash tidak dapat diubah.";
                return result;
            }

            Db.PaymentTerms.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data syarat pembayaran berhasil dihapus.";
        return result;
    }

    private bool IsInitialExists(string initial, int id)
    {
        return Db.PaymentTerms.Any(x => x.Initial == initial && x.Id != id && x.IsActive);
    }
}