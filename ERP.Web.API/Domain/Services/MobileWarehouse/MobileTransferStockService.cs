using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileWarehouse
{
    public class MobileTransferStockService : GeneralService<MobileTransferStockHeader>, IMobileTransferStockService
    {
        public MobileTransferStockService(TenantContext db)
            :base(db)
        {

        }

        public SaveResult Approve(List<MobileTransferStockHeader> data, int userId)
        {
            var result = new SaveResult(false);

            var items = Db.Items.Where(x => x.IsActive).ToList();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                foreach (var itemData in data)
                {
                    var mtsDetailList = Db.MobileTransferStockDetails.Where(x => x.Code == itemData.Code).ToList();

                    var tsHeadData = Db.TransferStockHeaders.FirstOrDefault(x => x.Code == itemData.TransferCode);
                    var tsDetailList = Db.TransferStockDetails.Where(x => x.Code == itemData.TransferCode).ToList();

                    foreach (var itemDetail in mtsDetailList)
                    {
                        var tsDetailData = tsDetailList.FirstOrDefault(x => x.ItemId == itemDetail.ItemId);

                        if (itemDetail.UnitId != tsDetailData.UnitId)
                        {
                            result.Message = "Data transfer persediaan mobile tidak dapat disetujui karena terdapat unit yang tidak sama dengan.";
                            return result;
                        }

                        if(tsDetailData.Qty != itemDetail.RealizeQty)
                        {
                            tsDetailData.Qty = itemDetail.RealizeQty;
                            Db.TransferStockDetails.Update(tsDetailData);

                            tsHeadData.ApprovedBy = userId;
                            tsHeadData.ApprovedDate = DateTime.Now;
                            Db.TransferStockHeaders.Update(tsHeadData);
                        }
                    }

                    // Update origin transfer code mark to A first
                    if (tsHeadData.Type == "IN")
                    {
                        Db.Database.ExecuteSqlRaw(
                            @"UPDATE TBA
                        SET TBA.Mark = 'A'
                        FROM Inventory.TransferStockHeader TBA
                        WHERE EXISTS (
                            SELECT OriginTransferCode 
                            FROM Inventory.TransferStockHeader TBB
                            WHERE TBB.Code = {0}
                            AND TBB.OriginTransferCode = TBA.Code
                        )
                        AND TBA.Mark != 'V'", tsHeadData.Code);
                    }

                    Db.SaveChanges();

                    // Update origin transfer code mark to CMP
                    if (tsHeadData.Type == "IN")
                    {
                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Inventory.TransferStockHeader SET Mark='CMP' WHERE Code={0} AND Mark!='V'",
                            tsHeadData.OriginTransferCode);
                    }

                    // Execute sp_update_transfer_stock
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0}, {1}, {2}",
                        tsHeadData.Code, tsHeadData.Date, 0);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Data transfer persediaan mobile berhasil disetujui.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobileTransferStockHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                data = data.Where(x => x.Code.Contains(search) || x.TransferCode.Contains(search));

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public IEnumerable<VwMobileTransferStockDetail> GetDetailData(string code)
        {
            var data = Db.VwMobileTransferStockDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.LineNo);
        }

        public SaveResult Reject(List<MobileTransferStockHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            foreach (var item in data)
            {
                if (item.Mark == "REJ")
                {
                    result.Message = "Data transfer persediaan mobile tidak bisa ditolak karena dalam status ditolak.";
                    return result;
                }

                var tsData = Db.MobileTransferStockHeaders.FirstOrDefault(x => x.Code == item.Code);
                tsData.RejectedBy = userId;
                tsData.RejectedDate = DateTime.Now;
                tsData.Mark = "REJ";
                Db.MobileTransferStockHeaders.Update(tsData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data transfer persediaan mobile berhasil ditolak.";
            return result;
        }

        public SaveResult Update(MobileTransferStockRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var delDetails = Db.MobileTransferStockDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.MobileTransferStockDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.MobileTransferStockDetails.Add(new MobileTransferStockDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            OriginalQty = item.OriginalQty,
                            RealizeQty = item.RealizeQty,
                            UomId = item.UomId,
                            UnitId = item.UnitId
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.MobileTransferStockDetails.Update(item);
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
            result.Message = "Data transfer persediaan mobile berhasil diperbarui.";
            return result;
        }
    }
}
