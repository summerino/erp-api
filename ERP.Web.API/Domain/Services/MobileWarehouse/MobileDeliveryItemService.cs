using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileWarehouse;

public class MobileDeliveryItemService : GeneralService<MobileDeliveryItemHeader>, IMobileDeliveryItemService
{
    public MobileDeliveryItemService(TenantContext db)
        :base(db)
    {

    }

    public SaveResult Approve(List<MobileDeliveryItemHeader> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var uomList = Db.UoMConversions.ToList();

            foreach (var itemData in data)
            {
                var mdiDetailData = Db.MobileDeliveryItemDetails.Where(x => x.Code == itemData.Code).ToList();

                var dplHeadData = Db.DeliveryPlanHeaders.FirstOrDefault(x => x.Code == itemData.DlvPlanCode);
                var dplDetailList = Db.DeliveryPlanDetails.Where(x => x.Code == itemData.DlvPlanCode).ToList();
                var dplUndelivList = Db.DeliveryPlanUndeliveredItems.Where(x => x.Code == itemData.DlvPlanCode).ToList();
                var dplDetailItemList = Db.DeliveryPlanDetailItems.Where(x => x.Code == itemData.DlvPlanCode).ToList();

                foreach (var itemDetail in mdiDetailData)
                {
                    var qty = itemDetail.RealizeQty;
                    var isFailedtoSend = false;
                    var isBonus = false;
                    var (qtyFailedtoSend, uomFailedtoSend, unitFailedtoSend) = (0m, 0, 0);
                    //List<string> listDoCode = new();

                    var doHeadData = Db.SalesDeliveryHeaders.Where(x => dplDetailList.Select(y => y.TransCode).Contains(x.Code))
                        .OrderBy(x => x.Code).ToList();
                    foreach (var itemDoData in doHeadData)
                    {
                        var doDetailList = Db.SalesDeliveryDetails.Where(x => x.Code == itemDoData.Code).ToList();
                        var doFreeList = Db.SalesDeliveryDetailFreeGoods.Where(x => x.Code == itemDoData.Code).ToList();

                        if (!isFailedtoSend)
                        {
                            if (dplDetailItemList.Where(x => x.Type == 0).Select(y => y.ItemId).Contains(itemDetail.ItemId))
                            {
                                var dplDetailItemData = dplDetailItemList.First(x => x.ItemId == itemDetail.ItemId && x.UnitId == itemDetail.UnitId && x.Type == 0);

                                isFailedtoSend = qty < dplDetailItemData.Qty;
                                if (isFailedtoSend)
                                {
                                    qtyFailedtoSend = dplDetailItemData.Qty - qty;
                                    uomFailedtoSend = dplDetailItemData.UomId;
                                    unitFailedtoSend = dplDetailItemData.UnitId;
                                }
                                else
                                {
                                    qty -= dplDetailItemData.Qty;
                                }
                            }
                        }

                        if (!isFailedtoSend)
                        {
                            if (dplDetailItemList.Where(x => x.Type == 1).Select(y => y.ItemId).Contains(itemDetail.ItemId))
                            {
                                var dplDetailItemData = dplDetailItemList.First(x => x.ItemId == itemDetail.ItemId && x.UnitId == itemDetail.UnitId && x.Type == 1);

                                isFailedtoSend = qty < dplDetailItemData.Qty;
                                if (isFailedtoSend)
                                {
                                    qtyFailedtoSend = dplDetailItemData.Qty - qty;
                                    uomFailedtoSend = dplDetailItemData.UomId;
                                    unitFailedtoSend = dplDetailItemData.UnitId;
                                    isBonus = true;
                                }
                                else
                                {
                                    qty -= dplDetailItemData.Qty;
                                }
                            }
                        }

                        if (isFailedtoSend)
                        {
                            var dplDetailData = dplDetailList.FirstOrDefault(x => x.Code == itemData.DlvPlanCode && x.TransCode == itemDoData.Code);
                            var dplUndelivData = dplUndelivList.FirstOrDefault(x => x.ItemId == itemDetail.ItemId && x.DlvPlanDetailId == dplDetailData.Id);
                            var dplDetailItemData = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.DlvPlanDetailId == dplDetailData.Id);

                            if (dplUndelivData != null)
                            {
                                if (dplUndelivData.Qty + qtyFailedtoSend > dplDetailItemData.Qty)
                                {
                                    result.Message = @$"Data pengeluaran barang mobile gagal disetujui <br/>
                                                        karena terdapat barang dengan qty gagal kirim <br/>
                                                        melebihi qty diterima - {itemData.Code}.";
                                    return result;
                                }
                                //dplUndelivData.UomId = uomFailedtoSend;
                                //dplUndelivData.UnitId = unitFailedtoSend;
                                dplUndelivData.Qty += qtyFailedtoSend;
                                Db.DeliveryPlanUndeliveredItems.Update(dplUndelivData);
                            }
                            else
                            {
                                Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                                {
                                    Code = dplDetailData.Code,
                                    DlvPlanDetailId = dplDetailData.Id,
                                    LineNo = 1,
                                    ItemId = itemDetail.ItemId,
                                    UomId = uomFailedtoSend,
                                    UnitId = unitFailedtoSend,
                                    Qty = qtyFailedtoSend,
                                    WarehouseCode = itemDoData.WarehouseCode,
                                    Type = isBonus ? 1 : 0
                                });
                            }

                            dplDetailData.IsFailShipment = true;
                            dplDetailData.NotesFailShipment = $"Created from Mobile Delivery Item {itemData.Code}";
                            Db.DeliveryPlanDetails.Update(dplDetailData);

                            dplHeadData.UpdatedBy = userId;
                            dplHeadData.UpdatedDate = DateTime.Now;
                            dplHeadData.ApprovedBy = userId;
                            dplHeadData.ApprovedDate = DateTime.Now;
                            Db.DeliveryPlanHeaders.Update(dplHeadData);


                            if (isBonus)
                            {
                                var doFreeData = doFreeList.FirstOrDefault(x => x.ItemId == itemDetail.ItemId && x.UnitId == itemDetail.UnitId);
                                if (doFreeData == null) continue;
                                doFreeData.Qty -= qtyFailedtoSend;
                                Db.SalesDeliveryDetailFreeGoods.Update(doFreeData);
                            }
                            else
                            {
                                var doDetailData = doDetailList.FirstOrDefault(x => x.ItemId == itemDetail.ItemId && x.UnitId == itemDetail.UnitId);
                                if (doDetailData == null) continue;
                                doDetailData.Qty -= qtyFailedtoSend;
                                doDetailData.Total -= (doDetailData.NettPrice * qtyFailedtoSend);
                                Db.SalesDeliveryDetails.Update(doDetailData);

                                itemDoData.SubTotal -= (doDetailData.NettPrice * qtyFailedtoSend);
                                itemDoData.TaxAmount -= (doDetailData.TaxAmount * qtyFailedtoSend);
                                itemDoData.Total -= (doDetailData.NettPrice * qtyFailedtoSend);
                                itemDoData.Dpp -= (doDetailData.Dpp * qtyFailedtoSend);
                                Db.SalesDeliveryHeaders.Update(itemDoData);

                                Db.SaveChanges();

                                Db.Database.ExecuteSqlRaw(
                                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                    itemDoData.Code, itemDoData.Date, itemDoData.TransCode);

                                if (itemDoData.SrcTrans == 1)
                                {
                                    // Execute sp_update_so_dlv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", itemDoData.TransCode);
                                }
                                else
                                {
                                    // Execute sp_update_sr_rcv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", itemDoData.TransCode);
                                }

                                if (itemDoData.Mark == "INV")
                                {
                                    var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == itemDoData.Code);
                                    var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                    if (siHeadData.Total > 0)
                                    {
                                        siDetailData.SubTotal -= (doDetailData.NettPrice * qtyFailedtoSend);
                                        siDetailData.TaxAmount -= (doDetailData.TaxAmount * qtyFailedtoSend);
                                        siDetailData.Total -= (doDetailData.NettPrice * qtyFailedtoSend);
                                        siDetailData.Dpp -= (doDetailData.Dpp * qtyFailedtoSend);
                                        Db.SalesInvoiceDetails.Update(siDetailData);

                                        siHeadData.Total -= (doDetailData.NettPrice * qtyFailedtoSend);
                                        if (siHeadData.Total < siHeadData.PaidAmount)
                                        {
                                            result.Message = "Data pengeluaran barang mobile gagal disetujui karena terdapat total faktur lebih kecil dari total pembayaran.";
                                            return result;
                                        }
                                        Db.SalesInvoiceHeaders.Update(siHeadData);
                                    }

                                    // Update sales delivery to invoiced
                                    Db.Database.ExecuteSqlRaw("UPDATE Sales.SalesDeliveryHeader SET Mark='INV' WHERE Code={0}", itemDoData.Code);

                                    // Check all sales delivery are invoiced
                                    if (
                                        !Db.SalesDeliveryHeaders
                                            .Any(x => x.TransCode == itemDoData.TransCode && x.Mark != "INV"))
                                    {
                                        // Update sales order to closed
                                        Db.Database.ExecuteSqlRaw(
                                            "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", itemDoData.TransCode);
                                    }
                                    else
                                    {
                                        // Update sales order to partial receive or completed
                                        var soMark = Db.SalesOrderDetails.Any(x => x.Code == itemDoData.TransCode && x.Qty > x.QtyDlv)
                                            ? "PS"
                                            : "CMP";

                                        Db.Database.ExecuteSqlRaw(
                                            "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, itemDoData.TransCode);
                                    }
                                }
                            }
                        }
                    }
                }

                itemData.Mark = "APR";
                itemData.ApprovedBy = userId;
                itemData.ApprovedDate = dplHeadData.ApprovedDate;
                Db.MobileDeliveryItemHeaders.Update(itemData);
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
        result.Message = "Data pengeluaran barang mobile berhasil disetujui.";
        return result;
    }

    public SaveResult CheckUndelivData(List<MobileDeliveryItemHeader> data)
    {
        var result = new SaveResult(false);

        try
        {
            foreach (var itemData in data)
            {
                var mdiDetailData = Db.MobileDeliveryItemDetails.Where(x => x.Code == itemData.Code).ToList();

                var dplHeadData = Db.DeliveryPlanHeaders.FirstOrDefault(x => x.Code == itemData.DlvPlanCode);
                var dplDetailList = Db.DeliveryPlanDetails.Where(x => x.Code == itemData.DlvPlanCode).ToList();
                var dplUndelivList = Db.DeliveryPlanUndeliveredItems.Where(x => x.Code == itemData.DlvPlanCode).ToList();
                foreach (var itemDetail in mdiDetailData)
                {
                    var undelivData = dplUndelivList.FirstOrDefault(x => x.ItemId == itemDetail.ItemId && x.UnitId == itemDetail.UnitId);
                    if (undelivData != null)
                    {
                        var itemMaster = Db.Items.FirstOrDefault(x => x.Id == itemDetail.ItemId);
                        var unitMaster = Db.UoMConversions.FirstOrDefault(x => x.Id == itemDetail.UnitId);

                        result.Success = true;
                        result.Data = new
                        {
                            Code = itemData.Code,
                            ItemName = itemMaster.Name,
                            UnitName = unitMaster.UnitEquivalent,
                            Qty = undelivData.Qty
                        };

                        return result;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        return result;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwMobileDeliveryItemHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            data = data.Where(x => x.Code.Contains(search) || x.DlvPlanCode.Contains(search));

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

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

        foreach (var item in data)
        {
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
                        OriginalQty = item.OriginalQty,
                        RealizeQty = item.RealizeQty,
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