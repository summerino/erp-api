using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales;

public class DeliveryPlanService : GeneralService<DeliveryPlanHeader>, IDeliveryPlanService
{
    public DeliveryPlanService(TenantContext db)
        : base(db)
    {

    }
    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
    {
        var data = Db.VwDeliveryPlanHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.VehicleNo.Contains(search) || x.DriverInitial.Contains(search) ||
                    x.WarehouseInitial.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<DeliveryPlanDetail> GetDetailData(string code)
    {
        return Db.DeliveryPlanDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
    }

    public IEnumerable<DeliveryPlanUndeliveredItem> GetUndeliveredData()
    {
        return Db.DeliveryPlanUndeliveredItems.ToList();
    }
    public List<dynamic> GetRelatedTransactions(string code)
    {
        throw new NotImplementedException();
    }

    public DataSourceResult GetAllTransaction(string warehouseCode, IEnumerable<Filter> filter)
    {
        var data = (from dt in Db.VwSalesDeliveryHeaders
                    join dto in Db.VwSalesOrderHeaders on dt.TransCode equals dto.Code into dtos 
                    from dtosRes in dtos.DefaultIfEmpty()
                    join dtr in Db.VwSalesReturnHeaders on dt.TransCode equals dtr.Code into dtrs 
                    from dtrsRes in dtrs.DefaultIfEmpty()
                    where dt.WarehouseCode == warehouseCode && !dt.FromDirectInvoice &&
                    dt.Mark != "V" &&
                    (!(from ddp in Db.DeliveryPlanDetails
                       join dp in Db.DeliveryPlanHeaders on ddp.Code equals dp.Code
                       where dp.Mark != "V"
                       select ddp.TransCode).Contains(dt.Code)
                    || (from ddp in Db.DeliveryPlanDetails
                        join dp in Db.DeliveryPlanHeaders on ddp.Code equals dp.Code
                        where dp.Mark != "V" && ddp.FailedSendAll
                        select ddp.TransCode).Contains(dt.Code))
                    select new
                    {
                        dt.Code,
                        SoCode = dt.TransCode,
                        dt.Date,
                        dt.Mark,
                        Type = "Surat Jalan",
                        dt.CustName,
                        dt.CustAddress,
                        dt.CustArea,
                        SalesName = dt.SrcTrans == 1 ? dtosRes.SalesName : dtrsRes.SalesName
                    }).Union(from dt in Db.VwSalesInvoiceHeaders
                             join dtp in Db.VwSalesDeliveryHeaders on dt.Code equals dtp.Code
                             join dto in Db.VwSalesOrderHeaders on dtp.TransCode equals dto.Code
                             where dtp.WarehouseCode == warehouseCode && dt.FromDirectInvoice && 
                             dt.Mark != "V" &&
                             (!(from ddp in Db.DeliveryPlanDetails
                                join dp in Db.DeliveryPlanHeaders on ddp.Code equals dp.Code
                                where dp.Mark != "V"
                                select ddp.TransCode).Contains(dt.Code)
                            || (from ddp in Db.DeliveryPlanDetails
                                join dp in Db.DeliveryPlanHeaders on ddp.Code equals dp.Code
                                where dp.Mark != "V" && ddp.FailedSendAll
                                select ddp.TransCode).Contains(dt.Code))
                             select new
                             {
                                 dt.Code,
                                 dt.SoCode,
                                 dt.Date,
                                 dt.Mark,
                                 Type = "Penjualan Langsung",
                                 dt.CustName,
                                 dt.CustAddress,
                                 dt.CustArea,
                                 dto.SalesName
                             });

        return data.AsQueryable().ToDataSourceResult(0, data.Count(), filter, null);
    }

    public SaveResult Insert(DeliveryPlanRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Get new code
            var newCode = GetNewCode("DLV_PLAN_NUM_FMT", data.Date);

            // Insert header data
            data.Code = newCode;
            Db.DeliveryPlanHeaders.Add(data);

            // Insert detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                var nvData = Db.DeliveryPlanHeaders.Where(x => x.Mark != "V").ToList();
                if (Db.DeliveryPlanDetails.Any(x => x.TransCode == item.TransCode && nvData.Select(y => y.Code).Contains(x.Code) && !x.FailedSendAll))
                {
                    result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan yang sudah digunakan.";
                    return result;
                }

                var doData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                if (doData.Date > data.Date)
                {
                    result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan yang tanggalnya lebih besar dari rencana pengiriman.";
                    return result;
                }

                var newItem = new DeliveryPlanDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    TransCode = item.TransCode,
                    Volume = item.Volume,
                    Weight = item.Weight,
                    SrcTrans = item.SrcTrans,
                    IsFailShipment = item.IsFailShipment,
                    FailedSendAll = item.FailedSendAll,
                    NotesFailShipment = item.NotesFailShipment
                };

                Db.DeliveryPlanDetails.Add(newItem);

                Db.SaveChanges();

                var idNewItem = newItem.Id;

                var dlvHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                var dlvDetail = Db.SalesDeliveryDetails.Where(x => x.Code == dlvHeader.Code).ToList();
                var dlvDetailFree = Db.SalesDeliveryDetailFreeGoods.Where(x => x.Code == dlvHeader.Code).ToList();
                var soHeader = new SalesOrderHeader();
                var siHeadData = new SalesInvoiceHeader();

                short dtl = 0;
                short dtlf = 0;
                foreach (var itemDlvDetail in dlvDetail)
                {
                    Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                    {
                        Code = newCode,
                        DlvPlanDetailId = idNewItem,
                        LineNo = ++dtl,
                        TransDetailId = itemDlvDetail.Id,
                        ItemId = itemDlvDetail.ItemId,
                        UomId = itemDlvDetail.UomId,
                        UnitId = itemDlvDetail.UnitId,
                        Qty = itemDlvDetail.Qty,
                        Type = 0
                    });
                }

                foreach (var itemDlvDetail in dlvDetailFree)
                {
                    Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                    {
                        Code = newCode,
                        DlvPlanDetailId = idNewItem,
                        LineNo = ++dtlf,
                        TransDetailId = itemDlvDetail.Id,
                        ItemId = itemDlvDetail.ItemId,
                        UomId = itemDlvDetail.UomId,
                        UnitId = itemDlvDetail.UnitId,
                        Qty = itemDlvDetail.Qty,
                        Type = 1
                    });
                }

                short j = 0;
                if(item.UndeliveredItems.Any() && item.IsFailShipment)
                {
                    foreach (var uItem in item.UndeliveredItems)
                    {
                        Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                        {
                            Code = newCode,
                            DlvPlanDetailId = idNewItem,
                            LineNo = ++j,
                            ItemId = uItem.ItemId,
                            UomId = uItem.UomId,
                            UnitId = uItem.UnitId,
                            Qty = uItem.Qty,
                            WarehouseCode = uItem.WarehouseCode,
                            Type = uItem.Type
                        });

                        if (uItem.Type == 0)
                        {
                            if (dlvHeader.FromDirectInvoice)
                            {
                                var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                soDetail.Qty -= uItem.Qty;
                                soDetail.QtyDlv -= uItem.Qty;
                                soDetail.Total -= (soDetail.NettPrice * uItem.Qty);
                                Db.SalesOrderDetails.Update(soDetail);

                                soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                soHeader.SubTotal -= (soDetail.NettPrice * uItem.Qty);
                                soHeader.TaxAmount -= (soDetail.TaxAmount * uItem.Qty);
                                soHeader.ExemptTaxAmount -= (soDetail.ExemptTaxAmount * uItem.Qty);
                                soHeader.Total -= (soDetail.NettPrice * uItem.Qty);
                                soHeader.Dpp -= (soDetail.Dpp * uItem.Qty);
                                Db.SalesOrderHeaders.Update(soHeader);
                            }

                            var sdDetail = dlvDetail.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                            sdDetail.Qty -= uItem.Qty;
                            sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                            Db.SalesDeliveryDetails.Update(sdDetail);

                            dlvHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                            dlvHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                            dlvHeader.ExemptTaxAmount -= (sdDetail.ExemptTaxAmount * uItem.Qty);
                            dlvHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                            dlvHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                            Db.SalesDeliveryHeaders.Update(dlvHeader);

                            if (dlvHeader.Mark == "INV")
                            {
                                var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == dlvHeader.Code);
                                siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                if (siHeadData.Total > 0)
                                {
                                    siDetailData.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                    siDetailData.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                    siDetailData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    siDetailData.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                    Db.SalesInvoiceDetails.Update(siDetailData);

                                    siHeadData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    //if (siHeadData.Total < siHeadData.PaidAmount)
                                    //{
                                    //    result.Message = "Data pengeluaran barang mobile gagal disetujui karena terdapat total faktur lebih kecil dari total pembayaran.";
                                    //    return result;
                                    //}
                                    Db.SalesInvoiceHeaders.Update(siHeadData);
                                }
                            }
                        }
                        else
                        {
                            if (dlvHeader.FromDirectInvoice)
                            {
                                var soDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                soDetail.Qty -= uItem.Qty;
                                soDetail.QtyClosed -= uItem.Qty;
                                Db.SalesOrderDetailFreeGoods.Update(soDetail);
                            }

                            var sdDetail = dlvDetailFree.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                            sdDetail.Qty -= uItem.Qty;
                            Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                        }
                    }

                    if (dlvHeader.FromDirectInvoice)
                        RestoreWarehouseQtySO(soHeader.Code);

                    RestoreWarehouseQtyDO(dlvHeader.Code, dlvHeader.TransCode, dlvHeader.SrcTrans);

                    if (dlvHeader.Mark == "INV")
                        RestoreWarehouseQtySI(dlvHeader.Code, dlvHeader.TransCode, dlvHeader.SrcTrans);

                    // Save changes
                    Db.SaveChanges();

                    if (dlvHeader.SrcTrans == 1)
                    {
                        // Execute sp_update_so_dlv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", dlvHeader.TransCode);
                    }
                    else
                    {
                        // Execute sp_update_sr_dlv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", dlvHeader.TransCode);
                    }

                    if (dlvHeader.FromDirectInvoice)
                    {
                        Db.Database.ExecuteSqlRaw(
                                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                                    soHeader.Code, soHeader.Date);
                    }

                    Db.Database.ExecuteSqlRaw(
                                   "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                   dlvHeader.Code, dlvHeader.Date, dlvHeader.TransCode);

                    if (dlvHeader.Mark == "INV")
                    {
                        Db.Database.ExecuteSqlRaw(
                                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                                    siHeadData.Code, siHeadData.Date, dlvHeader.Code);
                    }
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data rencana pengiriman berhasil disimpan.";
        return result;
    }

    public SaveResult Update(DeliveryPlanRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            if (Db.DeliveryPlanHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data rencana pengiriman tidak bisa diubah karena data sudah ditandai sebagai void.";
                return result;
            }

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            Db.DeliveryPlanHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            var delDetails = Db.DeliveryPlanDetails
                .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            Db.DeliveryPlanDetails.RemoveRange(delDetails);

            var delUnDetails = Db.DeliveryPlanUndeliveredItems
                .Where(x => delDetails.Select(d => d.Id).Contains(x.DlvPlanDetailId))
                .ToList();

            Db.DeliveryPlanUndeliveredItems.RemoveRange(delUnDetails);

            var delDetailItem = Db.DeliveryPlanDetailItems
                .Where(x => delDetails.Select(d => d.Id).Contains(x.DlvPlanDetailId))
                .ToList();

            if (delDetailItem.Any() && delDetailItem != null)
            {
                foreach (var deletedItem in delDetailItem)
                {
                    var transCode = delDetails.FirstOrDefault(x => x.Id == deletedItem.DlvPlanDetailId)?.TransCode;
                    if (deletedItem.Type == 0)
                    {
                        var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                        sdDetail.Qty = deletedItem.Qty;
                        sdDetail.Total = (sdDetail.NettPrice * deletedItem.Qty);
                        Db.SalesDeliveryDetails.Update(sdDetail);
                    }
                    else
                    {
                        var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == transCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                        sdDetail.Qty = deletedItem.Qty;
                        Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                    }
                }
            }
                
            Db.DeliveryPlanDetailItems.RemoveRange(delDetailItem);

            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                var nvData = Db.DeliveryPlanHeaders.Where(x => x.Mark != "V" && x.Code != data.Code).ToList();
                if (Db.DeliveryPlanDetails.Any(x => x.TransCode == item.TransCode && nvData.Select(y => y.Code).Contains(x.Code) && !x.FailedSendAll))
                {
                    result.Message = "Data rencana pengiriman tidak bisa diperbarui karena terdapat surat jalan yang sudah digunakan.";
                    return result;
                }

                var doData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                if (doData.Date > data.Date)
                {
                    result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan yang tanggalnya lebih besar dari rencana pengiriman.";
                    return result;
                }

                if (item.Id < 0)
                {
                    var newItem = new DeliveryPlanDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        TransCode = item.TransCode,
                        Volume = item.Volume,
                        Weight = item.Weight,
                        SrcTrans = item.SrcTrans,
                        IsFailShipment = item.IsFailShipment,
                        FailedSendAll = item.FailedSendAll,
                        NotesFailShipment = item.NotesFailShipment
                    };

                    Db.DeliveryPlanDetails.Add(newItem);

                    Db.SaveChanges();

                    var idNewItem = newItem.Id;

                    var dlvHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                    var dlvDetail = Db.SalesDeliveryDetails.Where(x => x.Code == dlvHeader.Code).ToList();
                    var dlvDetailFree = Db.SalesDeliveryDetailFreeGoods.Where(x => x.Code == dlvHeader.Code).ToList();
                    var soHeader = new SalesOrderHeader();
                    var siHeadData = new SalesInvoiceHeader();

                    short dtl = 0;
                    short dtlf = 0;
                    foreach (var itemDlvDetail in dlvDetail)
                    {
                        Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                        {
                            Code = data.Code,
                            DlvPlanDetailId = idNewItem,
                            LineNo = ++dtl,
                            TransDetailId = itemDlvDetail.Id,
                            ItemId = itemDlvDetail.ItemId,
                            UomId = itemDlvDetail.UomId,
                            UnitId = itemDlvDetail.UnitId,
                            Qty = itemDlvDetail.Qty,
                            Type = 0
                        });
                    }

                    foreach (var itemDlvDetail in dlvDetailFree)
                    {
                        Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                        {
                            Code = data.Code,
                            DlvPlanDetailId = idNewItem,
                            LineNo = ++dtlf,
                            TransDetailId = itemDlvDetail.Id,
                            ItemId = itemDlvDetail.ItemId,
                            UomId = itemDlvDetail.UomId,
                            UnitId = itemDlvDetail.UnitId,
                            Qty = itemDlvDetail.Qty,
                            Type = 1
                        });
                    }

                    if (item.UndeliveredItems.Any() && item.IsFailShipment)
                    {
                        short j = 0;
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                            {
                                Code = data.Code,
                                DlvPlanDetailId = idNewItem,
                                LineNo = ++j,
                                ItemId = uItem.ItemId,
                                UomId = uItem.UomId,
                                UnitId = uItem.UnitId,
                                Qty = uItem.Qty,
                                WarehouseCode = uItem.WarehouseCode,
                                Type = uItem.Type
                            });

                            if (uItem.Type == 0)
                            {
                                if (dlvHeader.FromDirectInvoice)
                                {
                                    var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                    soDetail.Qty -= uItem.Qty;
                                    soDetail.QtyDlv -= uItem.Qty;
                                    soDetail.Total -= (soDetail.NettPrice * uItem.Qty);
                                    Db.SalesOrderDetails.Update(soDetail);

                                    soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                    soHeader.SubTotal -= (soDetail.NettPrice * uItem.Qty);
                                    soHeader.TaxAmount -= (soDetail.TaxAmount * uItem.Qty);
                                    soHeader.ExemptTaxAmount -= (soDetail.ExemptTaxAmount * uItem.Qty);
                                    soHeader.Total -= (soDetail.NettPrice * uItem.Qty);
                                    soHeader.Dpp -= (soDetail.Dpp * uItem.Qty);
                                    Db.SalesOrderHeaders.Update(soHeader);
                                }

                                var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                var sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);

                                sdDetail.Qty -= uItem.Qty;
                                sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                Db.SalesDeliveryDetails.Update(sdDetail);

                                sdHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                sdHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                sdHeader.ExemptTaxAmount -= (sdDetail.ExemptTaxAmount * uItem.Qty);
                                sdHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                                sdHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                Db.SalesDeliveryHeaders.Update(sdHeader);

                                if (sdHeader.Mark == "INV")
                                {
                                    var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                    siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                    if (siHeadData.Total > 0)
                                    {
                                        siDetailData.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                        siDetailData.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                        siDetailData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                        siDetailData.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                        Db.SalesInvoiceDetails.Update(siDetailData);

                                        siHeadData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                        //if (siHeadData.Total < siHeadData.PaidAmount)
                                        //{
                                        //    result.Message = "Data pengeluaran barang mobile gagal disetujui karena terdapat total faktur lebih kecil dari total pembayaran.";
                                        //    return result;
                                        //}
                                        Db.SalesInvoiceHeaders.Update(siHeadData);
                                    }
                                }
                            }
                            else
                            {
                                if (dlvHeader.FromDirectInvoice)
                                {
                                    var soDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                    soDetail.Qty -= uItem.Qty;
                                    soDetail.QtyClosed -= uItem.Qty;
                                    Db.SalesOrderDetailFreeGoods.Update(soDetail);
                                }

                                var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);

                                sdDetail.Qty -= uItem.Qty;
                                Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                            }
                        }
                    }

                    if (dlvHeader.FromDirectInvoice)
                        RestoreWarehouseQtySO(soHeader.Code);

                    RestoreWarehouseQtyDO(dlvHeader.Code, dlvHeader.TransCode, dlvHeader.SrcTrans);

                    if (dlvHeader.Mark == "INV")
                        RestoreWarehouseQtySI(dlvHeader.Code, dlvHeader.TransCode, dlvHeader.SrcTrans);

                    // Save changes
                    Db.SaveChanges();

                    if (dlvHeader.SrcTrans == 1)
                    {
                        // Execute sp_update_so_dlv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", dlvHeader.TransCode);
                    }
                    else
                    {
                        // Execute sp_update_sr_dlv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", dlvHeader.TransCode);
                    }

                    if (dlvHeader.FromDirectInvoice)
                    {
                        Db.Database.ExecuteSqlRaw(
                                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                                    soHeader.Code, soHeader.Date);
                    }

                    Db.Database.ExecuteSqlRaw(
                                   "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                   dlvHeader.Code, dlvHeader.Date, dlvHeader.TransCode);

                    if (dlvHeader.Mark == "INV")
                    {
                        Db.Database.ExecuteSqlRaw(
                                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                                    siHeadData.Code, siHeadData.Date, dlvHeader.Code);
                    }
                }
                else
                {
                    item.LineNo = ++i;

                    Db.DeliveryPlanDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;

                    var undelivItem = Db.DeliveryPlanUndeliveredItems
                        .Where(x => x.DlvPlanDetailId == item.Id && !item.UndeliveredItems.Select(y => y.Id).Contains(x.Id)).ToList();

                    Db.DeliveryPlanUndeliveredItems.RemoveRange(undelivItem);

                    if (undelivItem.Any() && undelivItem != null)
                    {
                        foreach (var deletedItem in undelivItem)
                        {
                            if (deletedItem.Type == 0)
                            {
                                var dpdItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.Code == deletedItem.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId && x.Type == deletedItem.Type);
                                var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                sdDetail.Qty = dpdItem.Qty;
                                sdDetail.Total = (sdDetail.NettPrice * dpdItem.Qty);
                                Db.SalesDeliveryDetails.Update(sdDetail);
                            }
                            else
                            {
                                var dpdItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.Code == deletedItem.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId && x.Type == deletedItem.Type);
                                var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                sdDetail.Qty = dpdItem.Qty;
                                Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                            }
                            Db.SaveChanges();
                        }
                    }

                    if (item.UndeliveredItems.Any() && item.IsFailShipment)
                    {
                        short j = 0;
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            uItem.LineNo = ++j;

                            var unItem = Db.DeliveryPlanUndeliveredItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                            if (unItem != null)
                            {
                                var lastQty = unItem.Qty;
                                unItem.Qty = uItem.Qty;

                                Db.DeliveryPlanUndeliveredItems.Update(unItem);
                                Db.Entry(uItem).Property(e => e.Code).IsModified = false;

                                var soHeader = new SalesOrderHeader();
                                var sdHeader = new SalesDeliveryHeader();
                                var siHeadData = new SalesInvoiceHeader();

                                if (uItem.Type == 0)
                                {
                                    var sdDetail = uItem?.DetailId == null ? Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId)
                                        : Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                    sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                                    var dpDetailItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                                    if (sdHeader.FromDirectInvoice)
                                    {
                                        var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                        if (dpDetailItem != null)
                                        {
                                            soDetail.Qty = dpDetailItem.Qty - uItem.Qty;
                                            soDetail.QtyDlv = dpDetailItem.Qty - uItem.Qty;
                                            soDetail.Total = (soDetail.NettPrice * dpDetailItem.Qty) - (soDetail.NettPrice * uItem.Qty);
                                        }
                                        else
                                        {
                                            soDetail.Qty -= uItem.Qty;
                                            soDetail.QtyDlv -= uItem.Qty;
                                            soDetail.Total -= (soDetail.NettPrice * uItem.Qty);
                                        }
                                        Db.SalesOrderDetails.Update(soDetail);

                                        soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                        soHeader.SubTotal += (soDetail.NettPrice * lastQty) - (soDetail.NettPrice * uItem.Qty);
                                        soHeader.TaxAmount += (soDetail.TaxAmount * lastQty) - (soDetail.TaxAmount * uItem.Qty);
                                        soHeader.ExemptTaxAmount += (soDetail.ExemptTaxAmount * lastQty) - (soDetail.ExemptTaxAmount * uItem.Qty);
                                        soHeader.Total += (soDetail.NettPrice * lastQty) - (soDetail.NettPrice * uItem.Qty);
                                        soHeader.Dpp += (soDetail.Dpp * lastQty) - (soDetail.Dpp * uItem.Qty);
                                        Db.SalesOrderHeaders.Update(soHeader);
                                    }

                                    if (dpDetailItem != null)
                                    {
                                        sdDetail.Qty = dpDetailItem.Qty - uItem.Qty;
                                        sdDetail.Total = (sdDetail.NettPrice * dpDetailItem.Qty) - (sdDetail.NettPrice * uItem.Qty);
                                    }
                                    else
                                    {
                                        sdDetail.Qty -= uItem.Qty;
                                        sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    }
                                    Db.SalesDeliveryDetails.Update(sdDetail);

                                    sdHeader.SubTotal += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.TaxAmount += (sdDetail.TaxAmount * lastQty) - (sdDetail.TaxAmount * uItem.Qty);
                                    sdHeader.ExemptTaxAmount += (sdDetail.ExemptTaxAmount * lastQty) - (sdDetail.ExemptTaxAmount * uItem.Qty);
                                    sdHeader.Total += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.Dpp += (sdDetail.Dpp * lastQty) - (sdDetail.Dpp * uItem.Qty);
                                    Db.SalesDeliveryHeaders.Update(sdHeader);

                                    Db.SaveChanges();

                                    if (sdHeader.Mark == "INV")
                                    {
                                        var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                        siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                        if (siHeadData.Total > 0)
                                        {
                                            siDetailData.SubTotal += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                            siDetailData.TaxAmount += (sdDetail.TaxAmount * lastQty) - (sdDetail.TaxAmount * uItem.Qty);
                                            siDetailData.Total += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                            siDetailData.Dpp += (sdDetail.Dpp * lastQty) - (sdDetail.Dpp * uItem.Qty);
                                            Db.SalesInvoiceDetails.Update(siDetailData);

                                            siHeadData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                            //if (siHeadData.Total < siHeadData.PaidAmount)
                                            //{
                                            //    result.Message = "Data pengeluaran barang mobile gagal disetujui karena terdapat total faktur lebih kecil dari total pembayaran.";
                                            //    return result;
                                            //}
                                            Db.SalesInvoiceHeaders.Update(siHeadData);
                                        }
                                    }
                                }
                                else
                                {
                                    var sdDetail = uItem?.DetailId == null ? Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId)
                                        : Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);
                                    var dpDetailItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                                    if (dpDetailItem != null)
                                    {
                                        sdDetail.Qty = dpDetailItem.Qty - uItem.Qty;
                                    }
                                    else
                                    {
                                        sdDetail.Qty -= uItem.Qty;
                                    }
                                    Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                                }

                                if (sdHeader.FromDirectInvoice)
                                    RestoreWarehouseQtySO(soHeader.Code);

                                RestoreWarehouseQtyDO(sdHeader.Code, sdHeader.TransCode, sdHeader.SrcTrans);

                                if (sdHeader.Mark == "INV")
                                    RestoreWarehouseQtySI(sdHeader.Code, sdHeader.TransCode, sdHeader.SrcTrans);

                                // Save changes
                                Db.SaveChanges();

                                if (sdHeader.SrcTrans == 1)
                                {
                                    // Execute sp_update_so_dlv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", sdHeader.TransCode);
                                }
                                else
                                {
                                    // Execute sp_update_sr_dlv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", sdHeader.TransCode);
                                }

                                if (sdHeader.FromDirectInvoice)
                                {
                                    Db.Database.ExecuteSqlRaw(
                                                "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                                                soHeader.Code, soHeader.Date);
                                }

                                Db.Database.ExecuteSqlRaw(
                                               "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                               sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

                                if (sdHeader.Mark == "INV")
                                {
                                    Db.Database.ExecuteSqlRaw(
                                                "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                                                siHeadData.Code, siHeadData.Date, sdHeader.Code);
                                }
                            }
                            else
                            {
                                Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                                {
                                    Code = data.Code,
                                    DlvPlanDetailId = item.Id,
                                    LineNo = ++j,
                                    ItemId = uItem.ItemId,
                                    UomId = uItem.UomId,
                                    UnitId = uItem.UnitId,
                                    Qty = uItem.Qty,
                                    WarehouseCode = uItem.WarehouseCode,
                                    Type = uItem.Type
                                });

                                var soHeader = new SalesOrderHeader();
                                var sdHeader = new SalesDeliveryHeader();
                                var siHeadData = new SalesInvoiceHeader();

                                if (uItem.Type == 0)
                                {
                                    var sdDetail = uItem?.DetailId == null ? Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId)
                                        : Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                    sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);

                                    if (sdHeader.FromDirectInvoice)
                                    {
                                        var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);
                                        soDetail.Qty -= uItem.Qty;
                                        soDetail.QtyDlv -= uItem.Qty;
                                        soDetail.Total -= (soDetail.NettPrice * uItem.Qty);
                                        Db.SalesOrderDetails.Update(soDetail);

                                        soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                        soHeader.SubTotal -= (soDetail.NettPrice * uItem.Qty);
                                        soHeader.TaxAmount -= (soDetail.TaxAmount * uItem.Qty);
                                        soHeader.ExemptTaxAmount -= (soDetail.ExemptTaxAmount * uItem.Qty);
                                        soHeader.Total -= (soDetail.NettPrice * uItem.Qty);
                                        soHeader.Dpp -= (soDetail.Dpp * uItem.Qty);
                                        Db.SalesOrderHeaders.Update(soHeader);
                                    }

                                    sdDetail.Qty -= uItem.Qty;
                                    sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    Db.SalesDeliveryDetails.Update(sdDetail);

                                    sdHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                    sdHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                    Db.SalesDeliveryHeaders.Update(sdHeader);

                                    Db.SaveChanges();

                                    if (sdHeader.Mark == "INV")
                                    {
                                        var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                        siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                        if (siHeadData.Total > 0)
                                        {
                                            siDetailData.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                            siDetailData.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                            siDetailData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                            siDetailData.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                            Db.SalesInvoiceDetails.Update(siDetailData);

                                            siHeadData.Total -= (sdDetail.NettPrice * uItem.Qty);
                                            //if (siHeadData.Total < siHeadData.PaidAmount)
                                            //{
                                            //    result.Message = "Data pengeluaran barang mobile gagal disetujui karena terdapat total faktur lebih kecil dari total pembayaran.";
                                            //    return result;
                                            //}
                                            Db.SalesInvoiceHeaders.Update(siHeadData);
                                        }
                                    }
                                }
                                else
                                {
                                    var sdDetail = uItem?.DetailId == null ? Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId)
                                        : Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);

                                    sdDetail.Qty -= uItem.Qty;
                                    Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                                }

                                if (sdHeader.FromDirectInvoice)
                                    RestoreWarehouseQtySO(soHeader.Code);

                                RestoreWarehouseQtyDO(sdHeader.Code, sdHeader.TransCode, sdHeader.SrcTrans);

                                if (sdHeader.Mark == "INV")
                                    RestoreWarehouseQtySI(sdHeader.Code, sdHeader.TransCode, sdHeader.SrcTrans);

                                // Save changes
                                Db.SaveChanges();

                                if (sdHeader.SrcTrans == 1)
                                {
                                    // Execute sp_update_so_dlv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", sdHeader.TransCode);
                                }
                                else
                                {
                                    // Execute sp_update_sr_dlv_qty
                                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", sdHeader.TransCode);
                                }

                                if (sdHeader.FromDirectInvoice)
                                {
                                    Db.Database.ExecuteSqlRaw(
                                                "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                                                soHeader.Code, soHeader.Date);
                                }

                                Db.Database.ExecuteSqlRaw(
                                               "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                               sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

                                if (sdHeader.Mark == "INV")
                                {
                                    Db.Database.ExecuteSqlRaw(
                                                "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                                                siHeadData.Code, siHeadData.Date, sdHeader.Code);
                                }
                            }
                        }
                    }
                }
            }

            transaction.Commit();

        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data rencana pengiriman berhasil diperbarui.";
        return result;
    }
    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.DeliveryPlanHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data rencana pengiriman tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Update header data
            data.Mark = "V";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data rencana pengiriman berhasil ditandai sebagai void.";
        return result;
    }

    private void RestoreWarehouseQtySO(string code)
    {
        var dlvSOData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code && x.Src == "SO").ToList();
        foreach (var itemData in dlvSOData)
        {
            var whQtyData = new Entity.Inventory.WarehouseQuantity();
            if (itemData.Type == "OO")
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnOrder = whQtyData.QtyOnOrder - itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        Db.SaveChanges();
    }

    private void RestoreWarehouseQtyDO(string code, string srcCode, short srcTrans)
    {
        var dlvSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code && x.Src == "DO").ToList();
        var transSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == srcCode).ToList();
        if (srcTrans == 1)
        {
            foreach (var itemData in dlvSMData)
            {
                var whQtyData = new Entity.Inventory.WarehouseQuantity();
                if (itemData.Type == "OH")
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnHand = whQtyData.QtyOnHand + itemData.BaseQty;
                }
                else if (itemData.Type == "OO")
                {
                    var itemTransSMData = transSMData.FirstOrDefault(x => x.ItemId == itemData.ItemId && x.UnitId == itemData.UnitId);
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemTransSMData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnOrder = whQtyData.QtyOnOrder + itemData.BaseQty;
                }
                else
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnTransit = whQtyData.QtyOnTransit - itemData.BaseQty;
                }
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        else
        {
            foreach (var itemData in dlvSMData)
            {
                var whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                if (itemData.Type == "OH")
                    whQtyData.QtyOnHand = whQtyData.QtyOnHand + itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        Db.SaveChanges();
    }

    private void RestoreWarehouseQtySI(string code, string srcCode, short srcTrans)
    {
        var dlvSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code && x.Src == "SI").ToList();
        var transSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == srcCode).ToList();
        if (srcTrans == 1)
        {
            foreach (var itemData in dlvSMData)
            {
                if (itemData.Type == "OTS")
                {
                    var whQtyData = new Entity.Inventory.WarehouseQuantity();
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnTransit = whQtyData.QtyOnTransit + itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
            }
        }
        Db.SaveChanges();
    }

}