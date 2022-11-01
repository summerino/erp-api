using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Services.Inventory;

public class TransferStockService : GeneralService<TransferStockHeader>, ITransferStockService
{
    public TransferStockService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwTransferStockHeaders.Where(x => x.IsConsignee == false).AsQueryable();

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
            if (IsWarehouseInDateIsInvalid(data))
            {
                result.Message = "Tanggal transfer persediaan masuk tidak boleh lebih kecil dari tanggal transfer persediaan keluar.";
                return result;
            }

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
                if (item.Qty <= 0)
                {
                    result.Message = "Qty harus lebih besar dari nol.";
                    return result;
                }

                // Checking qty and item existing on source warehouse
                if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId) && data.Type != "IN")
                {
                    existListItemId.Add(item.ItemId);
                }

                // Try to guessing qty amount after insert/update?
                if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty) && data.Type != "IN")
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
            var errorList = "";
            string itemName;
            if (existListItemId.Count > 0)
            {
                foreach (var itemId in existListItemId)
                {
                    itemName = items.Find(x => x.Id == itemId)?.Name;
                    errorList += $"<br/>&bull; Barang {itemName} tidak tersedia.";
                }
            }

            if (qtyListItemId.Count > 0)
            {
                foreach (var itemId in qtyListItemId)
                {
                    itemName = items.Find(x => x.Id == itemId)?.Name;
                    errorList += $"<br/>&bull; Qty barang {itemName} pada gudang asal tidak mencukupi.";
                }
            }

            if (!string.IsNullOrEmpty(errorList))
            {
                errorList = errorList[5..];

                result.Message = errorList;
                return result;
            }

            Db.SaveChanges();

            // Update origin transfer code mark to CMP
            if (data.Type == "IN")
            {
                Db.Database.ExecuteSqlRaw(
                    "UPDATE Inventory.TransferStockHeader SET Mark='CMP' WHERE Code={0} AND Mark!='V'",
                    data.OriginTransferCode);
            }

            // Execute sp_update_transfer_stock
            Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0}, {1}, {2}", 
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
        result.Message = "Data transfer persediaan berhasil disimpan.";
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
                result.Message = "Data transfer persediaan tidak bisa diubah karena sudah ditandai sebagai void.";
                return result;
            }

            if (IsWarehouseInDateIsInvalid(data))
            {
                result.Message = "Tanggal transfer persediaan masuk tidak boleh lebih kecil dari tanggal transfer persediaan keluar.";
                return result;
            }

            RestoreWarehouseQty(data.Code);

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            // Update header data
            Db.TransferStockHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Get detail data that exists in order before
            var delDetails = Db.TransferStockDetails.Where(d => d.Code == data.Code).ToList();

            // Delete detail data that exists in order before
            Db.TransferStockDetails.RemoveRange(delDetails);

            // Update detail data
            short i = 0;
            var qtyListItemId = new List<int>();
            var existListItemId = new List<int>();
            foreach (var item in data.ItemDetails)
            {
                // Checking qty
                if (item.Qty <= 0)
                {
                    result.Message = "Qty harus lebih besar dari nol.";
                    return result;
                }

                // Checking qty and item existing on source warehouse
                if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId) && data.Type != "IN")
                {
                    existListItemId.Add(item.ItemId);
                }

                // Try to guessing qty amount after insert/update?
                if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty) && data.Type != "IN")
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
            var errorList = "";
            string itemName;
            if (existListItemId.Count > 0)
            {
                foreach (var itemId in existListItemId)
                {
                    itemName = items.Find(x => x.Id == itemId)?.Name;
                    errorList += $"<br/>&bull; Barang {itemName} tidak tersedia.";
                }
            }

            if (qtyListItemId.Count > 0)
            {
                foreach (var itemId in qtyListItemId)
                {
                    itemName = items.Find(x => x.Id == itemId)?.Name;
                    errorList += $"<br/>&bull; Qty barang {itemName} pada gudang asal tidak mencukupi.";
                }
            }

            if (!string.IsNullOrEmpty(errorList))
            {
                errorList = errorList[5..];

                result.Message = errorList;
                return result;
            }

            // Update origin transfer code mark to A first
            if (data.Type == "IN")
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
                        AND TBA.Mark != 'V'", data.Code);
            }

            Db.SaveChanges();

            // Update origin transfer code mark to CMP
            if (data.Type == "IN")
            {
                Db.Database.ExecuteSqlRaw(
                    "UPDATE Inventory.TransferStockHeader SET Mark='CMP' WHERE Code={0} AND Mark!='V'",
                    data.OriginTransferCode);
            }

            // Execute sp_update_transfer_stock
            Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0}, {1}, {2}",
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
        result.Message = "Data transfer persediaan berhasil diperbarui.";
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
                result.Message = "Data transfer persediaan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Checking void ordered for type 1 only
            if (data.Type == "OUT" && IsInventoryInAlreadyVoid(data.Code))
            {
                result.Message = "Data transfer persediaan tidak bisa ditandai sebagai void karena transfer persediaan pada barang masuk masih aktif.";
                return result;
            }

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();

                // Update origin transfer code mark to A
                if (data.Type == "IN")
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Inventory.TransferStockHeader SET Mark='A' WHERE Code={0} AND Mark!='V'",
                        data.OriginTransferCode);
                }

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0}, {1}, {2}",
                    data.Code, data.Date, 1);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
        }

        result.Success = true;
        result.Message = "Data transfer persediaan berhasil ditandai sebagai void.";
        return result;
    }

    private bool IsWarehouseItemExists(string warehouseCode, int itemId)
    {
        return Db.WarehouseQuantities.Any(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId && x.QtyOnHand > 0);
    }

    private bool IsInventoryInAlreadyVoid(string code)
    {
        return Db.TransferStockHeaders.Any(x => x.OriginTransferCode == code && x.Mark != "V");
    }

    private bool IsWarehouseItemQtyExists(string warehouseCode, int itemId, int uomId, int unitId, decimal qty)
    {
        var m = Db.UoMConversions.Where(x => x.UomId == uomId).OrderBy(x => x.Seq).ToList();

        var unit = m.Find(x => x.Id == unitId);

        var data = Db.WarehouseQuantities.SingleOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId);

        if (data == null)
            return true;

        if (unit.IsBaseUnit)
        {
            return data.QtyOnHand - data.QtyOnOrder >= qty;
        }

        var val = 
            m.Where(x => x.Seq <= unit.Seq)
                .Aggregate<UoMConversion, decimal>(1, (current, x) => current * x.Conversion);

        var itemConverted = (data.QtyOnHand - data.QtyOnOrder) / val;

        return itemConverted >= qty;
    }

    private bool IsWarehouseInDateIsInvalid(TransferStockRequest data)
    {
        var result = false;

        if (data.Type == "IN" && Db.TransferStockHeaders.FirstOrDefault(x => x.Code == data.OriginTransferCode).Date > data.Date)
            result = true;

        return result;
    }

    private void RestoreWarehouseQty(string code)
    {
        var TSData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code).ToList();
        foreach (var itemData in TSData)
        {
            var whQtyData = new WarehouseQuantity();
            if (itemData.Type == "OH" && itemData.BaseQty < 0)
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnHand = whQtyData.QtyOnHand + Math.Abs(itemData.BaseQty);
                Db.WarehouseQuantities.Update(whQtyData);
            }
            else if (itemData.Type == "OH" && itemData.BaseQty > 0)
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnHand = whQtyData.QtyOnHand - itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
            else if (itemData.Type == "OT" && itemData.BaseQty > 0)
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnTransfer = whQtyData.QtyOnTransfer - itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
            else if (itemData.Type == "OT" && itemData.BaseQty < 0)
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnTransfer = whQtyData.QtyOnTransfer - itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        Db.SaveChanges();
    }
}