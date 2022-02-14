using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
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
            var data = from dt in Db.VwSalesDeliveryHeaders
                       where dt.WarehouseCode == warehouseCode
                       && dt.Mark != "V" && (!(from ddp in Db.DeliveryPlanDetails
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
                            Type = dt.FromDirectInvoice ? "Penjualan Langsung" : "Surat Jalan",
                            dt.CustName,
                            dt.CustAddress,
                            dt.CustArea
                        };

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
                                var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                var sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                                Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                {
                                    Code = newCode,
                                    DlvPlanDetailId = idNewItem,
                                    LineNo = j,
                                    TransDetailId = sdDetail.Id,
                                    ItemId = sdDetail.ItemId,
                                    UomId = sdDetail.UomId,
                                    UnitId = sdDetail.UnitId,
                                    Qty = sdDetail.Qty,
                                    Type = uItem.Type
                                });

                                sdDetail.Qty -= uItem.Qty;
                                sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                Db.SalesDeliveryDetails.Update(sdDetail);

                                sdHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                sdHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                sdHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                                sdHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                Db.SalesDeliveryHeaders.Update(sdHeader);

                                Db.Database.ExecuteSqlRaw(
                                       "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                       sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

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

                                if (sdHeader.Mark == "INV")
                                {
                                    var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                    var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

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
                                var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);
                                Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                {
                                    Code = newCode,
                                    DlvPlanDetailId = idNewItem,
                                    LineNo = j,
                                    TransDetailId = sdDetail.Id,
                                    ItemId = sdDetail.ItemId,
                                    UomId = sdDetail.UomId,
                                    UnitId = sdDetail.UnitId,
                                    Qty = sdDetail.Qty,
                                    Type = uItem.Type
                                });

                                sdDetail.Qty -= uItem.Qty;
                                Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                            }
                        }
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
                    .Where(x => delDetails.Select(d => d.Id).Contains(x.DlvPlanDetailId));

                Db.DeliveryPlanUndeliveredItems.RemoveRange(delUnDetails);

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
                                    var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                    var sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                                    Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                    {
                                        Code = data.Code,
                                        DlvPlanDetailId = idNewItem,
                                        LineNo = j,
                                        TransDetailId = sdDetail.Id,
                                        ItemId = sdDetail.ItemId,
                                        UomId = sdDetail.UomId,
                                        UnitId = sdDetail.UnitId,
                                        Qty = sdDetail.Qty,
                                        Type = uItem.Type
                                    });

                                    sdDetail.Qty -= uItem.Qty;
                                    sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    Db.SalesDeliveryDetails.Update(sdDetail);

                                    sdHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                    sdHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                                    sdHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                    Db.SalesDeliveryHeaders.Update(sdHeader);

                                    Db.Database.ExecuteSqlRaw(
                                           "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                           sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

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

                                    if (sdHeader.Mark == "INV")
                                    {
                                        var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                        var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

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
                                    var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);
                                    Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                    {
                                        Code = data.Code,
                                        DlvPlanDetailId = idNewItem,
                                        LineNo = j,
                                        TransDetailId = sdDetail.Id,
                                        ItemId = sdDetail.ItemId,
                                        UomId = sdDetail.UomId,
                                        UnitId = sdDetail.UnitId,
                                        Qty = sdDetail.Qty,
                                        Type = uItem.Type
                                    });

                                    sdDetail.Qty -= uItem.Qty;
                                    Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                                }
                            }
                        }
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.DeliveryPlanDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        if (item.UndeliveredItems.Any() && item.IsFailShipment)
                        {
                            short j = 0;
                            foreach (var uItem in item.UndeliveredItems)
                            {
                                uItem.LineNo = ++j;

                                var unItem = Db.DeliveryPlanUndeliveredItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id);
                                if (unItem != null)
                                {
                                    var lastQty = unItem.Qty;
                                    unItem.Qty = uItem.Qty;

                                    Db.DeliveryPlanUndeliveredItems.Update(unItem);
                                    Db.Entry(uItem).Property(e => e.Code).IsModified = false;

                                    if (uItem.Type == 0)
                                    {
                                        var sdDetail = uItem?.DetailId == null ? Db.SalesDeliveryDetails.FirstOrDefault(x => x.Code == item.TransCode && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId)
                                            : Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                        var sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                                        var dpDetailItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                                        if (dpDetailItem != null)
                                        {
                                            sdDetail.Qty = dpDetailItem.Qty - uItem.Qty;
                                            sdDetail.Total = (sdDetail.NettPrice * dpDetailItem.Qty) - (sdDetail.NettPrice * uItem.Qty);
                                        }
                                        else
                                        {
                                            Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                            {
                                                Code = data.Code,
                                                DlvPlanDetailId = item.Id,
                                                LineNo = j,
                                                TransDetailId = sdDetail.Id,
                                                ItemId = sdDetail.ItemId,
                                                UomId = sdDetail.UomId,
                                                UnitId = sdDetail.UnitId,
                                                Qty = sdDetail.Qty,
                                                Type = uItem.Type
                                            });

                                            sdDetail.Qty -= uItem.Qty;
                                            sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                        }
                                        Db.SalesDeliveryDetails.Update(sdDetail);

                                        sdHeader.SubTotal += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                        sdHeader.TaxAmount += (sdDetail.TaxAmount * lastQty) - (sdDetail.TaxAmount * uItem.Qty);
                                        sdHeader.Total += (sdDetail.NettPrice * lastQty) - (sdDetail.NettPrice * uItem.Qty);
                                        sdHeader.Dpp += (sdDetail.Dpp * lastQty) - (sdDetail.Dpp * uItem.Qty);
                                        Db.SalesDeliveryHeaders.Update(sdHeader);

                                        Db.Database.ExecuteSqlRaw(
                                               "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                               sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

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

                                        if (sdHeader.Mark == "INV")
                                        {
                                            var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                            var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

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
                                        var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);
                                        var dpDetailItem = Db.DeliveryPlanDetailItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id && x.ItemId == uItem.ItemId && x.UnitId == uItem.UnitId);

                                        if (dpDetailItem != null)
                                        {
                                            sdDetail.Qty = dpDetailItem.Qty - uItem.Qty;
                                        }
                                        else
                                        {
                                            Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                            {
                                                Code = data.Code,
                                                DlvPlanDetailId = item.Id,
                                                LineNo = j,
                                                TransDetailId = sdDetail.Id,
                                                ItemId = sdDetail.ItemId,
                                                UomId = sdDetail.UomId,
                                                UnitId = sdDetail.UnitId,
                                                Qty = sdDetail.Qty,
                                                Type = uItem.Type
                                            });

                                            sdDetail.Qty -= uItem.Qty;
                                        }
                                        Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
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

                                    if (uItem.Type == 0)
                                    {
                                        var sdDetail = Db.SalesDeliveryDetails.FirstOrDefault(x => x.Id == uItem.DetailId);
                                        var sdHeader = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == sdDetail.Code);
                                        Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                        {
                                            Code = data.Code,
                                            DlvPlanDetailId = item.Id,
                                            LineNo = j,
                                            TransDetailId = sdDetail.Id,
                                            ItemId = sdDetail.ItemId,
                                            UomId = sdDetail.UomId,
                                            UnitId = sdDetail.UnitId,
                                            Qty = sdDetail.Qty,
                                            Type = uItem.Type
                                        });

                                        sdDetail.Qty -= uItem.Qty;
                                        sdDetail.Total -= (sdDetail.NettPrice * uItem.Qty);
                                        Db.SalesDeliveryDetails.Update(sdDetail);

                                        sdHeader.SubTotal -= (sdDetail.NettPrice * uItem.Qty);
                                        sdHeader.TaxAmount -= (sdDetail.TaxAmount * uItem.Qty);
                                        sdHeader.Total -= (sdDetail.NettPrice * uItem.Qty);
                                        sdHeader.Dpp -= (sdDetail.Dpp * uItem.Qty);
                                        Db.SalesDeliveryHeaders.Update(sdHeader);

                                        Db.Database.ExecuteSqlRaw(
                                               "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                                               sdHeader.Code, sdHeader.Date, sdHeader.TransCode);

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

                                        if (sdHeader.Mark == "INV")
                                        {
                                            var siDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.DoCode == sdHeader.Code);
                                            var siHeadData = Db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == siDetailData.Code);

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
                                        var sdDetail = Db.SalesDeliveryDetailFreeGoods.FirstOrDefault(x => x.Id == uItem.DetailId);
                                        Db.DeliveryPlanDetailItems.Add(new DeliveryPlanDetailItem
                                        {
                                            Code = data.Code,
                                            DlvPlanDetailId = item.Id,
                                            LineNo = j,
                                            TransDetailId = sdDetail.Id,
                                            ItemId = sdDetail.ItemId,
                                            UomId = sdDetail.UomId,
                                            UnitId = sdDetail.UnitId,
                                            Qty = sdDetail.Qty,
                                            Type = uItem.Type
                                        });

                                        sdDetail.Qty -= uItem.Qty;
                                        Db.SalesDeliveryDetailFreeGoods.Update(sdDetail);
                                    }
                                }
                            }
                        }
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

    }
}
