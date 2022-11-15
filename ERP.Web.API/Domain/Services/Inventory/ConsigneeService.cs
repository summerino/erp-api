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

public class ConsigneeService : GeneralService<TransferStockHeader>, IConsigneeService
{
    public ConsigneeService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwTransferStockHeaders.Where(x => x.IsConsignee == true).AsQueryable();

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

    public SaveResult Insert(TransferStockRequest data)
    {
        var result = new SaveResult(false);

        var items = Db.Items.Where(x => x.IsActive).ToList();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Get new code
            var newCode = GetNewCode("CNEE_NUM_FMT", data.Date);

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
                if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId))
                {
                    existListItemId.Add(item.ItemId);
                }

                // Try to guessing qty amount after insert/update?
                if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty))
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
        result.Message = "Data konsinyasi berhasil disimpan.";
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
                result.Message = "Data konsinyasi tidak bisa diubah karena sudah ditandai sebagai void.";
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
                if (!IsWarehouseItemExists(data.WarehouseCodeFrom, item.ItemId))
                {
                    existListItemId.Add(item.ItemId);
                }

                // Try to guessing qty amount after insert/update?
                if (!IsWarehouseItemQtyExists(data.WarehouseCodeFrom, item.ItemId, item.UomId, item.UnitId, item.Qty))
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
        result.Message = "Data konsinyasi berhasil diperbarui.";
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
                result.Message = "Data konsinyasi tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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
        result.Message = "Data konsinyasi berhasil ditandai sebagai void.";
        return result;
    }

    private bool IsWarehouseItemExists(string warehouseCode, int itemId)
    {
        return Db.WarehouseQuantities.Any(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId && x.QtyOnHand > 0);
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
                decimal qtyAvailable = data[0].QtyOnHand;
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
                decimal qtyAvailable = data[0].QtyOnHand;
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