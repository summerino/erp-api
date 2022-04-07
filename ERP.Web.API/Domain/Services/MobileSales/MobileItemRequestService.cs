using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model.MobileSales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileSales;

public class MobileItemRequestService : GeneralService<MobileItemRequestHeader>, IMobileItemRequestService
{
    public MobileItemRequestService(TenantContext db)
        :base(db)
    {

    }
    public SaveResult Approve(List<MobileItemRequest> data, int userId, string date, string whCode, string notes)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

        var items = Db.Items.Where(x => x.IsActive).ToList();

        var vDate = Convert.ToDateTime(date);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            foreach (var itemRequest in data)
            {
                // Get new code
                var newCode = GetNewCode("TS_NUM_FMT", vDate);

                // EMP data
                var empData = Db.Employees.FirstOrDefault(x => x.Id == itemRequest.SalesmanId);

                if(empData == null)
                    return new SaveResult(false, "Data penjual tidak ditemukan.");

                if(string.IsNullOrEmpty(empData.WarehouseCode))
                    return new SaveResult(false, "Data gudang penjual kosong.");

                // Insert header data
                var headData = new TransferStockHeader
                {
                    Code = newCode,
                    Date = vDate,
                    Type = "DT",
                    WarehouseCodeFrom = whCode,
                    WarehouseCodeTo = empData.WarehouseCode,
                    Notes = notes,
                    IsConsignee = false,
                    Mark = "A",
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.Now,
                    ApprovedBy = userId,
                    ApprovedDate = DateTime.Now
                };
                Db.TransferStockHeaders.Add(headData);

                // Insert detail data
                short i = 0;
                var qtyListItemId = new List<int>();
                var existListItemId = new List<int>();
                var itemData = Db.MobileItemRequestDetails.Where(x => x.Code == itemRequest.Code).ToList();
                foreach (var item in itemData)
                {
                    var currentItem = items.FirstOrDefault(x => x.Id == item.ItemId);
                    // Checking qty
                    if (item.Qty <= 0)
                    {
                        result.Message = "Qty harus lebih besar dari nol.";
                        return result;
                    }

                    // Checking qty and item existing on source warehouse
                    if (!IsWarehouseItemExists(whCode, item.ItemId))
                    {
                        existListItemId.Add(item.ItemId);
                    }

                    // Try to guessing qty amount after insert/update?
                    if (!IsWarehouseItemQtyExists(whCode, item.ItemId, (int)currentItem.UomId, item.UnitId, item.Qty))
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
                        UomId = (int)currentItem.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty
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

                var mirData = Db.MobileItemRequestHeaders.FirstOrDefault(x => x.Code == itemRequest.Code);
                mirData.TransferCode = newCode;
                mirData.Mark = "APR";
                mirData.ApprovedBy = userId;
                mirData.ApprovedDate = DateTime.Now;
                Db.MobileItemRequestHeaders.Update(mirData);

                Db.SaveChanges();

                // Execute sp_update_transfer_stock
                Db.Database.ExecuteSqlRaw("EXEC sp_update_transfer_stock {0}, {1}, {2}",
                    headData.Code, headData.Date, 0);
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Message = "Data permintaan barang mobile berhasil disetujui.";
        return result;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwMobileItemRequestHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.SalesmanInitial.Contains(search) || x.SalesmanName.Contains(search)
                    || x.Mark.Contains(search) || x.Status.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public IEnumerable<VwMobileItemRequestDetail> GetDetailData(string code)
    {
        var data = Db.VwMobileItemRequestDetails.Where(x => x.Code == code);

        return data.OrderBy(x => x.LineNo);
    }

    public SaveResult Reject(List<MobileItemRequest> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

        foreach (var item in data)
        {
            var mirData = Db.MobileItemRequestHeaders.FirstOrDefault(x => x.Code == item.Code);
            mirData.RejectedBy = userId;
            mirData.RejectedDate = DateTime.Now;
            mirData.Mark = "REJ";
            Db.MobileItemRequestHeaders.Update(mirData);
        }

        Db.SaveChanges();

        result.Success = true;
        result.Message = "Data permintaan barang mobile berhasil ditolak.";
        return result;
    }

    public SaveResult Update(MobileItemRequest data)
    {
        var result = new SaveResult(false);

        // Checking mark header data
        if (Db.MobileItemRequestHeaders.Any(x => x.Code == data.Code && x.Mark != "A"))
        {
            result.Message = "Data permintaan barang mobile tidak bisa diubah karena sudah tidak aktif.";
            return result;
        }

        // Get detail data that exists in order before
        var delDetails = Db.MobileItemRequestDetails.Where(d => d.Code == data.Code).ToList();

        // Delete detail data that exists in order before
        Db.MobileItemRequestDetails.RemoveRange(delDetails);

        if (data.ItemDetails.GroupBy(x => new { x.ItemId, x.UnitId }).Any(x => x.Count() > 1))
        {
            result.Message = "Terdapat barang dengan satuan yang sama pada bagian detail.";
            return result;
        }

        // Update detail data
        short i = 0;
        foreach (var item in data.ItemDetails)
        {
            // Checking qty
            if (item.Qty <= 0)
            {
                result.Message = "Qty harus lebih besar dari nol.";
                return result;
            }

            Db.MobileItemRequestDetails.Add(new MobileItemRequestDetail
            {
                Code = data.Code,
                LineNo = ++i,
                ItemId = item.ItemId,
                UnitId = item.UnitId,
                Qty = item.Qty
            });
        }

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data permintaan barang mobile berhasil diperbarui.";
        return result;
    }

    private bool IsWarehouseItemExists(string warehouseCode, int itemId)
    {
        return Db.WarehouseQuantities.Any(x => x.WarehouseCode == warehouseCode && x.ItemId == itemId && x.QtyOnHand > 0);
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
}