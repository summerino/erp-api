using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using Microsoft.Data.SqlClient;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobilePaymentMethodService : GeneralService<MobilePaymentMethod>, IMobilePaymentMethodService
    {
        public MobilePaymentMethodService(TenantContext db)
            :base(db)
        {

        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobilePaymentMethods.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Name.Contains(search) || x.CoaCode.Contains(search) || x.CoaName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public override SaveResult Insert(MobilePaymentMethod data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                Db.MobilePaymentMethods.Add(data);

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
            result.Message = "Data metode pembayaran berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(MobilePaymentMethod data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.MobilePaymentMethods.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data metode pembayaran diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.MobilePaymentMethods.Find(id);
            if (data != null)
            {
                try
                {
                    Db.MobilePaymentMethods.Remove(data);
                    Db.SaveChanges();
                }
                catch (Exception e)
                {
                    var ex = e?.InnerException as SqlException;
                    if (ex?.Number != 547) throw;

                    result.Message = "Data tidak bisa dihapus karena sedang digunakan oleh data lain.";
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data metode pembayaran dihapus.";
            return result;
        }
    }
}
