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

    public IEnumerable<VwDeliveryPlanDetail> GetDetailData(string code)
    {
        return Db.VwDeliveryPlanDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
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

                Db.SaveChanges();

                short j = 0;
                if(item.UndeliveredItems.Any() && item.IsFailShipment)
                {
                    foreach (var uItem in item.UndeliveredItems)
                    {
                        var dpUndelivItem = new DeliveryPlanUndeliveredItem
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
                        };
                        Db.DeliveryPlanUndeliveredItems.Add(dpUndelivItem);

                        UpdateWarehouseQty(item.TransCode, dpUndelivItem);

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
                                    if (siHeadData.Total < siHeadData.PaidAmount)
                                    {
                                        result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan / penjualan langsung yang total fakturnya lebih kecil dari nilai yang dibayarkan.";
                                        return result;
                                    }
                                    else if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                    {
                                        siHeadData.Mark = "PP";
                                    }
                                    else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                    {
                                        siHeadData.Mark = "CMP";
                                    }
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

            if (delUnDetails.Any() && delUnDetails != null)
            {
                foreach (var deletedItem in delUnDetails)
                {
                    var transCode = delDetails.FirstOrDefault(x => x.Id == deletedItem.DlvPlanDetailId)?.TransCode;
                    UpdateWarehouseQty(transCode, deletedItem, true);
                    Db.SaveChanges();
                }
            }

            Db.DeliveryPlanUndeliveredItems.RemoveRange(delUnDetails);

            var delDetailItem = Db.DeliveryPlanDetailItems
                .Where(x => delDetails.Select(d => d.Id).Contains(x.DlvPlanDetailId))
                .ToList();

            if (delDetailItem.Any() && delDetailItem != null)
            {
                foreach (var deletedItem in delDetailItem)
                {
                    var lastQty = 0m;
                    var transCode = delDetails.FirstOrDefault(x => x.Id == deletedItem.DlvPlanDetailId)?.TransCode;
                    if (deletedItem.Type == 0)
                    {
                        var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                        var sdHeadData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                        lastQty = sdDetail.Qty;
                        sdDetail.Qty = deletedItem.Qty;
                        sdDetail.Total = (sdDetail.NettPrice * deletedItem.Qty);
                        Db.SalesDeliveryDetails.Update(sdDetail);

                        sdHeadData.SubTotal += (sdDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                        sdHeadData.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty) - (sdDetail.TaxAmount * lastQty);
                        sdHeadData.ExemptTaxAmount += (sdDetail.ExemptTaxAmount * deletedItem.Qty) - (sdDetail.ExemptTaxAmount * lastQty);
                        sdHeadData.Total += (sdDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                        sdHeadData.Dpp += (sdDetail.Dpp * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                        Db.SalesDeliveryHeaders.Update(sdHeadData);

                        if (sdHeadData.FromDirectInvoice)
                        {
                            var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Id == sdDetail.SoDetailId && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                            soDetail.Qty += deletedItem.Qty - lastQty;
                            soDetail.QtyDlv += deletedItem.Qty - lastQty;
                            soDetail.Total += (soDetail.NettPrice * deletedItem.Qty) + (sdDetail.NettPrice * lastQty); ;
                            Db.SalesOrderDetails.Update(soDetail);

                            var soHeadData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == soDetail.Code);
                            soHeadData.SubTotal += (soDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                            soHeadData.TaxAmount += (soDetail.TaxAmount * deletedItem.Qty) - (sdDetail.TaxAmount * lastQty);
                            soHeadData.ExemptTaxAmount += (soDetail.ExemptTaxAmount * deletedItem.Qty) - (sdDetail.ExemptTaxAmount * lastQty);
                            soHeadData.Total += (soDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                            soHeadData.Dpp += (soDetail.Dpp * deletedItem.Qty) - (sdDetail.Dpp * lastQty);
                            Db.SalesOrderHeaders.Update(soHeadData);
                        }

                        if (sdHeadData.Mark == "INV")
                        {
                            var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeadData.Code);
                            var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                            if (siHeadData.Total > 0)
                            {
                                siDetailData.SubTotal += (sdDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                                siDetailData.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty) - (sdDetail.TaxAmount * lastQty);
                                siDetailData.Total += (sdDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                                siDetailData.Dpp += (sdDetail.Dpp * deletedItem.Qty) - (sdDetail.Dpp * lastQty);
                                Db.SalesInvoiceDetails.Update(siDetailData);

                                siHeadData.Total += (sdDetail.NettPrice * deletedItem.Qty) - (sdDetail.NettPrice * lastQty);
                                if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                {
                                    siHeadData.Mark = "PP";
                                }
                                else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                {
                                    siHeadData.Mark = "CMP";
                                }
                                Db.SalesInvoiceHeaders.Update(siHeadData);
                            }
                        }
                    }
                    else
                    {
                        var sdDetailFree = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == transCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                        lastQty = sdDetailFree.Qty;
                        sdDetailFree.Qty = deletedItem.Qty - lastQty;
                        Db.SalesDeliveryDetailFreeGoods.Update(sdDetailFree);
                        var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == sdDetailFree.DlvOrderDetailId);
                        var sdHeadData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);

                        if (sdHeadData.FromDirectInvoice)
                        {
                            var soDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Id == sdDetail.SoDetailId && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                            soDetail.Qty += deletedItem.Qty - lastQty;
                            soDetail.QtyClosed += deletedItem.Qty - lastQty;
                            Db.SalesOrderDetailFreeGoods.Update(soDetail);
                        }
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

                    Db.SaveChanges();

                    if (item.UndeliveredItems.Any() && item.IsFailShipment)
                    {
                        short j = 0;
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            var dpUndelivItem = new DeliveryPlanUndeliveredItem
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
                            };
                            Db.DeliveryPlanUndeliveredItems.Add(dpUndelivItem);

                            UpdateWarehouseQty(item.TransCode, dpUndelivItem);

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
                                        if (siHeadData.Total < siHeadData.PaidAmount)
                                        {
                                            result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan / penjualan langsung yang total fakturnya lebih kecil dari nilai yang dibayarkan.";
                                            return result;
                                        }
                                        else if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                        {
                                            siHeadData.Mark = "PP";
                                        }
                                        else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                        {
                                            siHeadData.Mark = "CMP";
                                        }
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
                        }
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
                        var soHeader = new SalesOrderHeader();
                        var sdHeader = new SalesDeliveryHeader();
                        var siHeadData = new SalesInvoiceHeader();

                        foreach (var deletedItem in undelivItem)
                        {
                            UpdateWarehouseQty(item.TransCode, deletedItem, true);
                            if (deletedItem.Type == 0)
                            {
                                var dpdItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.Code == deletedItem.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId && x.Type == deletedItem.Type);
                                var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                if (sdHeader.FromDirectInvoice)
                                {
                                    var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == sdHeader.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                    soDetail.Qty = dpdItem.Qty;
                                    soDetail.QtyDlv = dpdItem.Qty;
                                    soDetail.Total = (sdDetail.NettPrice * dpdItem.Qty);
                                    Db.SalesOrderDetails.Update(soDetail);

                                    soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                    soHeader.SubTotal += (soDetail.NettPrice * deletedItem.Qty);
                                    soHeader.TaxAmount += (soDetail.TaxAmount * deletedItem.Qty);
                                    soHeader.ExemptTaxAmount += (soDetail.ExemptTaxAmount * deletedItem.Qty);
                                    soHeader.Total += (soDetail.NettPrice * deletedItem.Qty);
                                    soHeader.Dpp += (soDetail.Dpp * deletedItem.Qty);
                                    Db.SalesOrderHeaders.Update(soHeader);
                                }

                                sdDetail.Qty = dpdItem.Qty;
                                sdDetail.Total = (sdDetail.NettPrice * dpdItem.Qty);
                                Db.SalesDeliveryDetails.Update(sdDetail);

                                sdHeader.SubTotal += (sdDetail.NettPrice * deletedItem.Qty);
                                sdHeader.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty);
                                sdHeader.ExemptTaxAmount += (sdDetail.ExemptTaxAmount * deletedItem.Qty);
                                sdHeader.Total += (sdDetail.NettPrice * deletedItem.Qty);
                                sdHeader.Dpp += (sdDetail.Dpp * deletedItem.Qty);
                                Db.SalesDeliveryHeaders.Update(sdHeader);

                                if (sdHeader.Mark == "INV")
                                {
                                    var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                    siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                    if (siHeadData.Total > 0)
                                    {
                                        siDetailData.SubTotal += (sdDetail.NettPrice * deletedItem.Qty);
                                        siDetailData.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty);
                                        siDetailData.Total += (sdDetail.NettPrice * deletedItem.Qty);
                                        siDetailData.Dpp += (sdDetail.Dpp * deletedItem.Qty);
                                        Db.SalesInvoiceDetails.Update(siDetailData);

                                        siHeadData.Total += (sdDetail.NettPrice * deletedItem.Qty);
                                        if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                        {
                                            siHeadData.Mark = "PP";
                                        }
                                        else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                        {
                                            siHeadData.Mark = "CMP";
                                        }
                                        Db.SalesInvoiceHeaders.Update(siHeadData);
                                    }
                                }
                            }
                            else
                            {
                                var dpdItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.Code == deletedItem.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId && x.Type == deletedItem.Type);
                                var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                sdDetail.Qty = dpdItem.Qty;
                                Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                            }
                        }
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
                    }

                    if (item.UndeliveredItems.Any() && item.IsFailShipment)
                    {
                        var soHeader = new SalesOrderHeader();
                        var sdHeader = new SalesDeliveryHeader();
                        var siHeadData = new SalesInvoiceHeader();

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

                                UpdateWarehouseQty(item.TransCode, unItem, true);
                                UpdateWarehouseQty(item.TransCode, uItem);

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

                                            siHeadData.Total += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                            if (siHeadData.Total < siHeadData.PaidAmount)
                                            {
                                                result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan / penjualan langsung yang total fakturnya lebih kecil dari nilai yang dibayarkan.";
                                                return result;
                                            }
                                            else if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                            {
                                                siHeadData.Mark = "PP";
                                            }
                                            else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                            {
                                                siHeadData.Mark = "CMP";
                                            }
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
                            }
                            else
                            {
                                var dpUndelivItem = new DeliveryPlanUndeliveredItem
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
                                };

                                Db.DeliveryPlanUndeliveredItems.Add(dpUndelivItem);

                                UpdateWarehouseQty(item.TransCode, dpUndelivItem);

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
                                            if (siHeadData.Total < siHeadData.PaidAmount)
                                            {
                                                result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan / penjualan langsung yang total fakturnya lebih kecil dari nilai yang dibayarkan.";
                                                return result;
                                            }
                                            else if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                            {
                                                siHeadData.Mark = "PP";
                                            }
                                            else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                            {
                                                siHeadData.Mark = "CMP";
                                            }
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
                            }
                        }
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

            var detailData = Db.DeliveryPlanDetails.Where(x => x.Code == code).ToList();
            foreach (var item in detailData)
            {
                var undelivItem = Db.DeliveryPlanUndeliveredItems
                        .Where(x => x.DlvPlanDetailId == item.Id).ToList();

                if (undelivItem.Any() && undelivItem != null)
                {
                    var soHeader = new SalesOrderHeader();
                    var sdHeader = new SalesDeliveryHeader();
                    var siHeadData = new SalesInvoiceHeader();

                    foreach (var deletedItem in undelivItem)
                    {
                        UpdateWarehouseQty(item.TransCode, deletedItem, true);

                        if (deletedItem.Type == 0)
                        {
                            var dpdItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.Code == deletedItem.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId && x.Type == deletedItem.Type);
                            var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                            sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                            if (sdHeader.FromDirectInvoice)
                            {
                                var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == sdHeader.Code && x.ItemId == deletedItem.ItemId && x.UnitId == deletedItem.UnitId);
                                soDetail.Qty = dpdItem.Qty;
                                soDetail.QtyDlv = dpdItem.Qty;
                                soDetail.Total = (sdDetail.NettPrice * dpdItem.Qty);
                                Db.SalesOrderDetails.Update(soDetail);

                                soHeader = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                                soHeader.SubTotal += (soDetail.NettPrice * deletedItem.Qty);
                                soHeader.TaxAmount += (soDetail.TaxAmount * deletedItem.Qty);
                                soHeader.ExemptTaxAmount += (soDetail.ExemptTaxAmount * deletedItem.Qty);
                                soHeader.Total += (soDetail.NettPrice * deletedItem.Qty);
                                soHeader.Dpp += (soDetail.Dpp * deletedItem.Qty);
                                Db.SalesOrderHeaders.Update(soHeader);
                            }

                            sdDetail.Qty = dpdItem.Qty;
                            sdDetail.Total = (sdDetail.NettPrice * dpdItem.Qty);
                            Db.SalesDeliveryDetails.Update(sdDetail);

                            sdHeader.SubTotal += (sdDetail.NettPrice * deletedItem.Qty);
                            sdHeader.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty);
                            sdHeader.ExemptTaxAmount += (sdDetail.ExemptTaxAmount * deletedItem.Qty);
                            sdHeader.Total += (sdDetail.NettPrice * deletedItem.Qty);
                            sdHeader.Dpp += (sdDetail.Dpp * deletedItem.Qty);
                            Db.SalesDeliveryHeaders.Update(sdHeader);

                            if (sdHeader.Mark == "INV")
                            {
                                var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

                                if (siHeadData.Total > 0)
                                {
                                    siDetailData.SubTotal += (sdDetail.NettPrice * deletedItem.Qty);
                                    siDetailData.TaxAmount += (sdDetail.TaxAmount * deletedItem.Qty);
                                    siDetailData.Total += (sdDetail.NettPrice * deletedItem.Qty);
                                    siDetailData.Dpp += (sdDetail.Dpp * deletedItem.Qty);
                                    Db.SalesInvoiceDetails.Update(siDetailData);

                                    siHeadData.Total += (sdDetail.NettPrice * deletedItem.Qty);
                                    if (siHeadData.PaidAmount > 0 && siHeadData.Total > siHeadData.PaidAmount)
                                    {
                                        siHeadData.Mark = "PP";
                                    }
                                    else if (siHeadData.PaidAmount > 0 && siHeadData.Total == siHeadData.PaidAmount)
                                    {
                                        siHeadData.Mark = "CMP";
                                    }
                                    Db.SalesInvoiceHeaders.Update(siHeadData);
                                }
                            }
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
                }
            }

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data rencana pengiriman berhasil ditandai sebagai void.";
        return result;
    }

    private void UpdateWarehouseQty(string code, DeliveryPlanUndeliveredItem undeliveredItem, bool isRestore = false)
    {
        if (undeliveredItem.Type == 0)
        {
            var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == code && x.ItemId == undeliveredItem.ItemId && x.UnitId == undeliveredItem.UnitId);
            var sdHeadData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == undeliveredItem.UnitId);
            var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == undeliveredItem.WarehouseCode && x.ItemId == undeliveredItem.ItemId);

            if (uom.IsBaseUnit)
            {
                if (isRestore)
                {
                    stock.QtyOnTransit += undeliveredItem.Qty;
                    stock.QtyOnOrder -= undeliveredItem.Qty;
                    stock.QtyOnHand -= undeliveredItem.Qty;
                }
                else
                {
                    stock.QtyOnTransit -= undeliveredItem.Qty;
                    stock.QtyOnOrder += undeliveredItem.Qty;
                    stock.QtyOnHand += undeliveredItem.Qty;
                }
                
            }
            else
            {
                var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                var baseQty = undeliveredItem.Qty * multipliedQty;
                if (isRestore)
                {
                    stock.QtyOnTransit += baseQty;
                    stock.QtyOnOrder -= baseQty;
                    stock.QtyOnHand -= baseQty;
                }
                else
                {
                    stock.QtyOnTransit -= baseQty;
                    stock.QtyOnOrder += baseQty;
                    stock.QtyOnHand += baseQty;
                }
            }

            if (sdHeadData.FromDirectInvoice)
            {
                if (uom.IsBaseUnit)
                {
                    if (isRestore)
                        stock.QtyOnOrder += undeliveredItem.Qty;
                    else
                        stock.QtyOnOrder -= undeliveredItem.Qty;
                }
                else
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    var baseQty = undeliveredItem.Qty * multipliedQty;

                    if (isRestore)
                        stock.QtyOnOrder += baseQty;
                    else
                        stock.QtyOnOrder -= baseQty;

                }
            }

            if (sdHeadData.Mark == "INV")
            {
                if (uom.IsBaseUnit)
                {
                    if(isRestore)
                        stock.QtyOnTransit -= undeliveredItem.Qty;
                    else
                        stock.QtyOnTransit += undeliveredItem.Qty;
                }
                else
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    var baseQty = undeliveredItem.Qty * multipliedQty;

                    if (isRestore)
                        stock.QtyOnTransit -= baseQty;
                    else
                        stock.QtyOnTransit += baseQty;
                }
            }
            Db.WarehouseQuantities.Update(stock);
        }
        else
        {
            var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Code == code && x.ItemId == undeliveredItem.ItemId && x.UnitId == undeliveredItem.UnitId);
            var sdHeadData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == undeliveredItem.UnitId);
            var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == undeliveredItem.WarehouseCode && x.ItemId == undeliveredItem.ItemId);

            if (uom.IsBaseUnit)
            {
                if (isRestore)
                {
                    stock.QtyOnTransit += undeliveredItem.Qty;
                    stock.QtyOnOrder -= undeliveredItem.Qty;
                    stock.QtyOnHand -= undeliveredItem.Qty;
                }
                else
                {
                    stock.QtyOnTransit -= undeliveredItem.Qty;
                    stock.QtyOnOrder += undeliveredItem.Qty;
                    stock.QtyOnHand += undeliveredItem.Qty;
                }

            }
            else
            {
                var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                var baseQty = undeliveredItem.Qty * multipliedQty;
                if (isRestore)
                {
                    stock.QtyOnTransit += baseQty;
                    stock.QtyOnOrder -= baseQty;
                    stock.QtyOnHand -= baseQty;
                }
                else
                {
                    stock.QtyOnTransit -= baseQty;
                    stock.QtyOnOrder += baseQty;
                    stock.QtyOnHand += baseQty;
                }
            }

            if (sdHeadData.FromDirectInvoice)
            {
                if (uom.IsBaseUnit)
                {
                    if (isRestore)
                        stock.QtyOnOrder += undeliveredItem.Qty;
                    else
                        stock.QtyOnOrder -= undeliveredItem.Qty;
                }
                else
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    var baseQty = undeliveredItem.Qty * multipliedQty;

                    if (isRestore)
                        stock.QtyOnOrder += baseQty;
                    else
                        stock.QtyOnOrder -= baseQty;

                }
            }

            if (sdHeadData.Mark == "INV")
            {
                if (uom.IsBaseUnit)
                {
                    if (isRestore)
                        stock.QtyOnTransit -= undeliveredItem.Qty;
                    else
                        stock.QtyOnTransit += undeliveredItem.Qty;
                }
                else
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == undeliveredItem.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    var baseQty = undeliveredItem.Qty * multipliedQty;

                    if (isRestore)
                        stock.QtyOnTransit -= baseQty;
                    else
                        stock.QtyOnTransit += baseQty;
                }
            }
            Db.WarehouseQuantities.Update(stock);
        }
    }
}