using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.AssetManagement;
using ERP.Web.API.Domain.Interfaces.AssetManagement;

namespace ERP.Web.API.Domain.Services.AssetManagement
{
    public class FixedAssetService : GeneralService<FixedAsset>, IFixedAssetService
    {
        public FixedAssetService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwFixedAssets.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.PurchaseDate == searchDate || x.StartDepreciateOn == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search) || x.SupName.Contains(search) ||
                        x.PurchaseOrderNo.Contains(search) || x.InvoiceNo.Contains(search) || x.PaymentVoucherNo.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.AssetTypes.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public DataSourceResult GetListHistories(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.FixedAssetHistories;

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(FixedAsset data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("FIXED_ASSET_NUM_FMT", data.PurchaseDate);

                // Insert header data
                data.Code = newCode;
                Db.FixedAssets.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Data = data.Code;
            result.Success = true;
            result.Message = "Data aktiva tetap berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(FixedAsset data)
        {
            var result = new SaveResult(false);

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            // Update data
            Db.FixedAssets.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.BookValue).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();
            result.Data = data.Code;
            result.Success = true;
            result.Message = "Data aktiva tetap berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.FixedAssets.SingleOrDefault(x=>x.Code.Equals(code));
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data order pembelian tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data aktiva tetap berhasil dihapus.";
            return result;
        }
    }
}
