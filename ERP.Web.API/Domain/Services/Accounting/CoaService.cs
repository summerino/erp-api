using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Accounting;

public class CoaService : GeneralService<Coa>, ICoaService
{
    public CoaService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwCoas.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string mobileLastSync)
    {
        var data = Db.Coas.Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(mobileLastSync))
        {
            switch (mobileLastSync.Length)
            {
                case 19:
                    mobileLastSync += ".0000";
                    break;
                case 21:
                    mobileLastSync += "000";
                    break;
                case 22:
                    mobileLastSync += "00";
                    break;
                case 23:
                    mobileLastSync += "0";
                    break;
            }
            data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(mobileLastSync, "yyyy-MM-ddTHH:mm:ss.ffff", null));
        }

        var dataT = data;

        data = data.Where(x => !dataT.Select(t => t.ParentId).Contains(x.Id));

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public DataSourceResult GetListsNonSysPar(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.Coas.Where(x => x.IsActive);
        var dataT = data;

        var sysParData = Db.SystemParameters.Where(x => x.Code.Contains("COA")).ToList();

        data = data.Where(x => !dataT.Select(t => t.ParentId).Contains(x.Id));

        data = data.Where(x => !sysParData.Select(c => c.Value).Contains(x.Code));

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public DataSourceResult GetListParents(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.Coas.Where(x => x.IsActive);
        var dataT = data;

        data = data.Where(x => dataT.Select(t => t.ParentId).Contains(x.Id));

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public override SaveResult Insert(Coa data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking code already exists or not
            if (IsCoaExists(data.Code, 0))
            {
                result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
                return result;
            }

            //Set default seq value
            data.IsSeq = int.MaxValue;
            data.IsDetSeq = int.MaxValue;

            // Insert data
            if (data.ParentId != null)
            {
                var dataParent = Db.Coas.FirstOrDefault(x => x.Id == data.ParentId);
                if (dataParent.ShowInMobile)
                {
                    result.Message = $"Akun {dataParent.Code} - {dataParent.Name} tidak bisa dijadikan induk akun karena sudah ditampilkan di mobile.";
                    return result;
                }
                data.Deep = dataParent.Deep == null ? 1 : dataParent.Deep + 1;
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
        result.Data = data.Code;
        result.Message = "Data akun berhasil disimpan.";
        return result;
    }

    public override SaveResult Update(Coa data)
    {
        var result = new SaveResult(false);

        // Checking code already exists or not
        if (IsCoaExists(data.Code, data.Id))
        {
            result.Message = "Kode sudah terdaftar. Tolong gunakan kode lain.";
            return result;
        }

        var oldData = Db.Coas.AsNoTracking().FirstOrDefault(x => x.Code == data.Code);
        if (oldData.IsCode != data.IsCode)
            data.IsSeq = int.MaxValue;

        if (oldData.IsDetCode != data.IsDetCode)
            data.IsDetSeq = int.MaxValue;

        // Checking if coa type is cash bank
        if (data.TypeId == 2)
        {
            result.Message = "Data akun dengan tipe kas & bank tidak bisa diperbarui.";
            return result;
        }

        // Update data
        if (data.ParentId != null)
        {
            var dataParent = Db.Coas.FirstOrDefault(x => x.Id == data.ParentId);
            if (dataParent.ShowInMobile)
            {
                result.Message = $"Akun {dataParent.Code} - {dataParent.Name} tidak bisa dijadikan induk akun karena sudah ditampilkan di mobile.";
                return result;
            }
            data.Deep = dataParent.Deep == null ? 1 : dataParent.Deep + 1;
        }
        Db.Coas.Update(data);            
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data akun berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(int id, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.Coas.Find(id);
        if (data != null)
        {
            // Checking active
            if (data.IsActive == false)
            {
                result.Message = "Tidak bisa menonaktifkan data akun karena data sudah nonaktif.";
                return result;
            }

            //Check if any promo already using this coa
            if (Db.PromoHeaders.Any(x => x.CoaCost == data.Code))
            {
                result.Message = "Tidak bisa menghapus data akun karena telah digunakan pada data promo.";
                return result;
            }

            // Checking if coa type is cash bank
            if (data.TypeId == 2)
            {
                result.Message = "Data akun dengan tipe kas & bank tidak bisa dihapus.";
                return result;
            }

            // Delete data
            Db.Coas.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data akun berhasil dihapus.";
        return result;
    }

    private bool IsCoaExists(string code, int id)
    {
        return Db.Coas.Any(x => x.Code == code && x.Id != id);
    }

    public DataSourceResult GetListGeneralJournal()
    {
        var data = Db.Coas.Where(x => x.IsActive && x.TypeId != 1);
        var dataT = data;

        var specialCOA = new[] { "AP_COA", "AP_DFR_COA", "AR_COA", "CHQ_AP_COA", "CHQ_AR_COA",
            "CM_AP_COA", "COGS_COA", "CROSS_COA", "DM_AR_COA", "DEP_CUST_COA",
            "DEP_SUP_COA", "EP_AP_COA", "INVENTORY_COA", "SENT_ITEM_COA" };

        var sysParData = Db.SystemParameters.Where(x => specialCOA.Contains(x.Code)).ToList();

        data = data.Where(x => !dataT.Select(t => t.ParentId).Contains(x.Id));

        data = data.Where(x => !sysParData.Select(c => c.Value).Contains(x.Code));

        return data.ToDataSourceResult(0, -1, null, null);
    }

    public DataSourceResult GetListGeneralTransaction()
    {
        var data = Db.Coas.Where(x => x.IsActive && x.TypeId != 1);
        var dataT = data;

        var specialCOA = new[] { "AP_COA", "AP_DFR_COA", "AR_COA", "CHQ_AP_COA", "CHQ_AR_COA",
            "CM_AP_COA", "COGS_COA", "CROSS_COA", "DM_AR_COA", "DEP_CUST_COA",
            "DEP_SUP_COA", "EP_AP_COA", "INVENTORY_COA", "SENT_ITEM_COA",
            "RETAINED_EARNING_COA", "SLS_COA", "SLS_DISC_COA", "SLS_RTN_COA", "CM_AP_COA"};

        var sysParData = Db.SystemParameters.Where(x => specialCOA.Contains(x.Code)).ToList();

        data = data.Where(x => !dataT.Select(t => t.ParentId).Contains(x.Id));

        data = data.Where(x => !sysParData.Select(c => c.Value).Contains(x.Code));

        return data.ToDataSourceResult(0, -1, null, null);
    }
}