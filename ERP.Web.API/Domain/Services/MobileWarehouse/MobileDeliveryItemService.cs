using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.MobileWarehouse
{
    public class MobileDeliveryItemService : GeneralService<MobileDeliveryItemHeader>, IMobileDeliveryItemService
    {
        public MobileDeliveryItemService(TenantContext db)
            :base(db)
        {

        }

        public SaveResult Approve(List<MobileDeliveryItemHeader> data, int userId)
        {
            throw new NotImplementedException();
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobileDeliveryItemHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                data = data.Where(x => x.Code.Contains(search));

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public IEnumerable<VwMobileDeliveryItemDetail> GetDetailData(string code)
        {
            var data = Db.VwMobileDeliveryItemDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.LineNo);
        }

        public SaveResult Reject(List<MobileDeliveryItemHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            foreach (var item in data)
            {
                if (item.Mark == "REJ")
                {
                    result.Message = "Data pengeluaran barang mobile tidak bisa ditolak karena dalam status ditolak.";
                    return result;
                }

                var dlvData = Db.MobileDeliveryItemHeaders.FirstOrDefault(x => x.Code == item.Code);
                dlvData.RejectedBy = userId;
                dlvData.RejectedDate = DateTime.Now;
                dlvData.Mark = "REJ";
                Db.MobileDeliveryItemHeaders.Update(dlvData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data pengeluaran barang mobile berhasil ditolak.";
            return result;
        }

        public SaveResult Update(MobileDeliveryItemRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get detail data that exists in dlv before
                var delDetails = Db.MobileDeliveryItemDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in dlv before
                Db.MobileDeliveryItemDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.MobileDeliveryItemDetails.Add(new MobileDeliveryItemDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            Qty = item.Qty,
                            UomId = item.UomId,
                            UnitId = item.UnitId
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.MobileDeliveryItemDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Save changes
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
            result.Message = "Data pengeluaran barang mobile berhasil diperbarui.";
            return result;
        }
    }
}
