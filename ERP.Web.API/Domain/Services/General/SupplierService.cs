using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Domain.Services.General;

public class SupplierService : GeneralService<Supplier>, ISupplierService
{
    public SupplierService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwSuppliers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Code.Contains(search) || x.Initial.Contains(search) || x.Name.Contains(search) || x.TypeName.Contains(search) ||
                x.Address1.Contains(search) || x.Phone.Contains(search) || x.Email.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.Suppliers.Where(x => x.IsActive);

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public Supplier FindByCode(string code)
    {
        return Db.Suppliers.Find(code);
    }

    public override SaveResult Insert(Supplier data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, ""))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Get new code
            var newCode = GetNewCode("SUP_NUM_FMT", data.CreatedDate);

            // Insert data
            data.Code = newCode;
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
        result.Data = data.Code;
        result.Message = "Data pemasok berhasil disimpan.";
        return result;
    }

    public override SaveResult Update(Supplier data)
    {
        var result = new SaveResult(false);

        // Checking initial already exists or not
        if (IsInitialExists(data.Initial, data.Code))
        {
            result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
            return result;
        }

        // Update data
        Db.Suppliers.Update(data);
        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data pemasok berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.Suppliers.Find(code);
        if (data != null)
        {
            //Check if any purchase order already using this supplier
            if (Db.PurchaseOrderHeaders.Any(x => x.SupCode == data.Code))
            {
                result.Message = "Tidak bisa menghapus data pemasok karena telah digunakan pada data order pembelian.";
                return result;
            }

            Db.Suppliers.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data pemasok berhasil dihapus.";
        return result;
    }

    private bool IsInitialExists(string initial, string code)
    {
        return Db.Customers.Any(x => x.Initial == initial && x.Code != code);
    }
}