using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Inventory
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

            var items = Db.Items.Where(x => x.IsActive).ToList();

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
                var qtyListItemId = new List<int>();
                var existListItemId = new List<int>();
                foreach (var item in data.ItemDetails)
                {
                    // Checking qty
                    if (item.Qty < 0)
                    {
                        result.Message = "Qty tidak boleh negative.";
                        return result;
                    }

                    // Checking qty and item existing on source warehouse
                    if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId) && data.Type != 2)
                    {
                        existListItemId.Add(item.ItemId);
                    }

                    // Try to guessing qty amount after insert/update?
                    if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty) && data.Type != 2)
                    {
                        if (existListItemId.Count > 0)
                        {
                            // existListItemId & qtyListItemId array are same validation but message is different
                            if (!existListItemId.Exists(x => x == item.ItemId))
                            {
                                qtyListItemId.Add(item.ItemId);
                            }
                        }
                        else
                        {
                            qtyListItemId.Add(item.ItemId);
                        }
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

                // Summary
                string errorList = "";
                string itemName = "";
                if (existListItemId.Count > 0 && existListItemId != null)
                {
                    for (int j = 0; j < existListItemId.Count; j++)
                    {
                        itemName = items.Find(x => x.Id == existListItemId[j]).Name;
                        if (errorList == "")
                        {
                            errorList = "&bull; Barang " + itemName + " tidak tersedia.";
                        }
                        else
                        {
                            errorList += "<br/>&bull; Barang " + itemName + " tidak tersedia.";
                        }
                    }
                }

                if (qtyListItemId.Count > 0 && qtyListItemId != null)
                {
                    for (int j = 0; j < qtyListItemId.Count; j++)
                    {
                        itemName = items.Find(x => x.Id == qtyListItemId[j]).Name;
                        if (errorList == "")
                        {
                            errorList = "&bull; Qty barang " + itemName + " pada gudang asal tidak mencukupi.";
                        }
                        else
                        {
                            errorList += "<br/>&bull; Qty barang " + itemName + " pada gudang asal tidak mencukupi.";
                        }
                    }
                }

                if (errorList != "")
                {
                    result.Message = errorList;
                    return result;
                }

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0},{1},{2}", 
                    data.Code, data.Date, 0);

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

            var items = Db.Items.Where(x => x.IsActive).ToList();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.TransferStockHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data transfer stok tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.TransferStockHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.TransferStockDetails
                    .Where(d => d.Code == data.Code).ToList();

                // Delete detail data that exists in order before
                Db.TransferStockDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                var qtyListItemId = new List<int>();
                var existListItemId = new List<int>();
                foreach (var item in data.ItemDetails)
                {
                    // Checking qty
                    if (item.Qty < 0)
                    {
                        result.Message = "Qty tidak boleh negative.";
                        return result;
                    }

                    // Checking qty and item existing on source warehouse
                    if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId) && data.Type != 2)
                    {
                        existListItemId.Add(item.ItemId);
                    }

                    // Try to guessing qty amount after insert/update?
                    if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty) && data.Type != 2)
                    {
                        if (existListItemId.Count > 0)
                        {
                            // existListItemId & qtyListItemId array are same validation but message is different
                            if (!existListItemId.Exists(x => x == item.ItemId))
                            {
                                qtyListItemId.Add(item.ItemId);
                            }
                        }
                        else
                        {
                            qtyListItemId.Add(item.ItemId);
                        }
                    }

                    Db.TransferStockDetails.Add(new TransferStockDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Notes = item.Notes
                    });
                }

                // Summary
                string errorList = "";
                string itemName = "";
                if (existListItemId.Count > 0 && existListItemId != null)
                {
                    for (int j = 0; j < existListItemId.Count; j++)
                    {
                        itemName = items.Find(x => x.Id == existListItemId[j]).Name;
                        if (errorList == "")
                        {
                            errorList = "&bull; Barang " + itemName + " tidak tersedia.";
                        }
                        else
                        {
                            errorList += "<br/>&bull; Barang " + itemName + " tidak tersedia.";
                        }
                    }
                }

                if (qtyListItemId.Count > 0 && qtyListItemId != null)
                {
                    for (int j = 0; j < qtyListItemId.Count; j++)
                    {
                        itemName = items.Find(x => x.Id == qtyListItemId[j]).Name;
                        if (errorList == "")
                        {
                            errorList = "&bull; Qty barang " + itemName + " pada gudang asal tidak mencukupi.";
                        }
                        else
                        {
                            errorList += "<br/>&bull; Qty barang " + itemName + " pada gudang asal tidak mencukupi.";
                        }
                    }
                }

                if (errorList != "")
                {
                    result.Message = errorList;
                    return result;
                }

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0},{1},{2}",
                    data.Code, data.Date, 0);

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

                // Checking void ordered for type 1 only
                if (data.Type == 1 && !IsInventoryInAlreadyVoid(data.Code))
                {
                    result.Message = "Data transfer stok tidak bisa ditandai sebagai void karena transfer persediaan pada barang masuk masih aktif.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0},{1},{2}",
                    data.Code, data.Date, 1);
            }

            result.Success = true;
            result.Message = "Data transfer stok berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsWarehouseItemExists(string warehouseCode, int itemId)
        {
            return Db.WarehouseQuantities.Any(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId && x.QtyOnHand > 0);
        }

        private bool IsInventoryInAlreadyVoid(string code)
        {
            return Db.TransferStockHeaders.Any(x => x.OriginTransferCode == code && x.Mark == "V");
        }

        private bool IsWarehouseItemQtyExists(string warehouseCode, int itemId, int uomId, int unitId, decimal qty)
        {
            decimal val = 1;
            var m = Db.UoMConversions.Where(x => x.UomId == uomId).OrderBy(x => x.Seq).ToList();

            var unit = m.Find(x => x.Id == unitId);

            var data = Db.WarehouseQuantities.Where(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId).ToList();

            if (unit.IsBaseUnit)
            {
                if (data.Count > 0)
                {
                    decimal qtyAvailable = data[0].QtyOnHand - data[0].QtyOnOrder;
                    if (qtyAvailable < qty)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                for (int i = 0; i < m.Count; i++)
                {
                    if (m[i].Seq <= unit.Seq)
                    {
                        val *= m[i].Conversion;
                    }
                }

                if (data.Count > 0)
                {
                    decimal qtyAvailable = data[0].QtyOnHand - data[0].QtyOnOrder;
                    decimal itemConverted = qtyAvailable / val;

                    if (itemConverted < qty)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
