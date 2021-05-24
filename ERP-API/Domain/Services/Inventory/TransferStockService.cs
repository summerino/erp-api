using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domain.Services.Inventory
{
    public class TransferStockService : GeneralService<TransferStockHeader>, ITransferStockService
    {
        public TransferStockService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwTransferStockHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.OriginTransferCode.Contains(search) || x.WarehouseInitialFrom.Contains(search) ||
                        x.WarehouseInitialTo.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwTransferStockDetail> GetDetailData(string code)
        {
            var data = Db.VwTransferStockDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var data = from ts in Db.TransferStockHeaders
                       where ts.OriginTransferCode == code && ts.Mark == "A"
                       select new { ts.Code, ts.Date, ts.Mark };

            return data.ToDynamicList();
        }

        public SaveResult Insert(TransferStockRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("TS_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.TransferStockHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    // Checking qty
                    if (item.Qty < 0)
                    {
                        result.Message = "Qty tidak boleh negative.";
                        return result;
                    }

                    // Checking qty and item existing on source warehouse
                    if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId))
                    {
                        result.Message = "Barang tidak tersedia.";
                        return result;
                    }

                    Db.TransferStockDetails.Add(new TransferStockDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Notes = item.Notes
                    });
                }

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0},{1},{2},{3},{4},{5}", 
                    data.Code, data.Date, data.Type, data.WarehouseCodeFrom, data.WarehouseCodeTo, 0);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data transfer stok berhasil disimpan.";
            return result;
        }

        public SaveResult Update(TransferStockRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.TransferStockHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data transfer stok tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                Db.TransferStockHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.TransferStockDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.TransferStockDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    // Checking qty
                    if (item.Qty < 0)
                    {
                        result.Message = "Qty tidak boleh negative.";
                        return result;
                    }

                    // Checking qty and item existing on source warehouse
                    if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId))
                    {
                        result.Message = "Barang tidak tersedia.";
                        return result;
                    }

                    if (item.Id == 0)
                    {
                        Db.TransferStockDetails.Add(new TransferStockDetail
                        {
                            Code = item.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            Notes = item.Notes
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.TransferStockDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0},{1},{2},{3},{4},{5}",
                    data.Code, data.Date, data.Type, data.WarehouseCodeFrom, data.WarehouseCodeTo, 0);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data transfer stok berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.TransferStockHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data transfer stok tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data transfer stok berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsWarehouseItemExists(string warehouseCode, int itemId)
        {
            return Db.WarehouseQuantities.Any(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId && x.QtyOnHand > 0);
        }
    }
}
