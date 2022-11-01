using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesDeliveryService : GeneralService<SalesDeliveryHeader>, ISalesDeliveryService
{
    public SalesDeliveryService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwSalesDeliveryHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.CustCode.StartsWith(search) || x.CustName.Contains(search) ||
                    x.TransCode == search || x.ShippedInitial.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<VwSalesDeliveryDetail> GetDetailData(string code)
    {
        return Db.VwSalesDeliveryDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
    }

    public List<dynamic> GetRelatedTransactions(string code)
    {
        var siD = from dt in Db.SalesInvoiceDetails
            where dt.DoCode == code
            select dt.Code;

        var dpD = from dlv in Db.DeliveryPlanDetails
            where dlv.TransCode == code
            select dlv.Code;

        var data = (new[] { new { Code = "", Date = new DateTime(), Total = (decimal)0, Type = "" } })
            .Union(from siH in Db.SalesInvoiceHeaders
                where siD.Contains(siH.Code) && siH.Mark != "V"
                select new { siH.Code, siH.Date, siH.Total, Type = "Faktur" })
            .Union(from dpH in Db.DeliveryPlanHeaders
                where dpD.Contains(dpH.Code) && dpH.Mark != "V"
                select new { dpH.Code, dpH.Date, Total = (decimal)0, Type = "Rencana Pengiriman" })
            .Skip(1);

        return data.ToDynamicList();
    }

    public IEnumerable<SalesDeliveryHeader> GetUnInvoiceData(string soCode, string invCode)
    {
        var data = Db.SalesDeliveryHeaders.Where(x => x.TransCode == soCode);

        data = string.IsNullOrWhiteSpace(invCode)
            ? data.Where(x => x.Mark == "A")
            : data.Where(x => x.Mark == "A" ||
                              Db.SalesInvoiceDetails
                                  .Where(i => i.Code == invCode)
                                  .Select(i => i.DoCode).Contains(x.Code));

        return data;
    }

    public SaveResult Insert(SalesDeliveryRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            if (data.SrcTrans == 2)
            {
                var transData = Db.SalesReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan tidak ditemukan.";
                    return result;
                }

                // Checking sales return mark
                if (transData.Mark == "V")
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan sudah ditandai sebagai void.";
                    return result;
                }

                // Checking sales return date with sales delivery
                if (transData.Date > data.Date)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan mempunyai tanggal lebih besar.";
                    return result;
                }
            }
            else
            {
                var transData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan tidak ditemukan.";
                    return result;
                }

                // Checking sales order mark
                if (transData.Mark is "V" or "CLS")
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking sales order date with sales delivery
                if (transData.Date > data.Date)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan mempunyai tanggal lebih besar.";
                    return result;
                }

                if (IsSaveAndDateInvalid(data))
                {
                    result.Message = "Tanggal Faktur Penjualan tidak boleh lebih kecil dari tanggal Surat Jalan.";
                    return result;
                }
            }

            // Checking warehouse qty is item is available or not
            var isQtyAvailable = IsQtyAvailable(null, data.WarehouseCode, data.ItemDetails);
            switch (isQtyAvailable)
            {
                case 1:
                    result.Message = "Terdapat barang yang tidak tersedia pada gudang yang dipilih.";
                    return result;
                case 2:
                    result.Message = "Terdapat barang yang qty-nya melebihi ketersediaan pada gudang yang dipilih.";
                    return result;
            }

            // Checking deliver qty is excess or not
            if (IsQtyExcess(null, data.SrcTrans, data.TransCode, data.ItemDetails))
            {
                result.Message = "Data pengiriman penjualan tidak bisa disimpan karena qty yg dikirim lebih besar dari qty yang tersedia.";
                return result;
            }

            var taxes = Db.Taxes.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();
            List<decimal> totalFinalDiscHeader = new();

            // Get new code
            var newCode = GetNewCode("DO_NUM_FMT", data.Date);

            data.Code = newCode;

            // Insert detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                if (data.SrcTrans == 1)
                {
                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);

                    if (data.IncludeTax)
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.Rate / 100)));
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.ExemptRate / 100)));
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.Rate / 100);
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.ExemptRate / 100);
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                    }

                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalFinalDiscHeader.Add(item.Qty * item.FinalDiscHeader);
                    totalTax.Add(item.Qty * item.TaxAmount);
                    totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                    totalDpp.Add(item.Qty * item.Dpp);
                }

                var deliveryDetail = new SalesDeliveryDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    SoDetailId = item.SoDetailId,
                    ItemId = item.ItemId,
                    Qty = item.Qty,
                    UomId = item.UomId,
                    UnitId = item.UnitId,
                    Length = item.Length,
                    Width = item.Width,
                    Height = item.Height,
                    Weight = item.Weight,
                    DimensionMeasurement = item.DimensionMeasurement,
                    WeightMeasurement = item.WeightMeasurement,
                    UnitPrice = item.UnitPrice,
                    Disc = item.Disc,
                    FinalDiscHeader = item.FinalDiscHeader,
                    TaxId = item.TaxId,
                    TaxAmount = item.TaxAmount,
                    ExemptTaxAmount = item.ExemptTaxAmount,
                    NettPrice = item.NettPrice,
                    Total = item.Total,
                    Dpp = item.Dpp
                };

                Db.SalesDeliveryDetails.Add(deliveryDetail);

                if (item.FreeItemDetails != null)
                {
                    Db.SaveChanges();
                }

                if (item.FreeItemDetails != null)
                {
                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            Db.SalesDeliveryDetailFreeGoods.Add(new SalesDeliveryDetailFreeGood
                            {
                                Code = newCode,
                                DlvOrderDetailId = deliveryDetail.Id,
                                LineNo = ++f,
                                PromoCode = freeItem.PromoCode,
                                ItemId = freeItem.ItemId,
                                UomId = freeItem.UomId,
                                UnitId = freeItem.UnitId,
                                Qty = freeItem.Qty,
                                UnitPrice = freeItem.UnitPrice,
                                CoaCode = freeItem.CoaCode
                            });

                            var orderFreeDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Id == freeItem.Id);
                            orderFreeDetail.QtyClosed += freeItem.Qty;
                            Db.SalesOrderDetailFreeGoods.Update(orderFreeDetail);
                        }
                    }
                    Db.SaveChanges();
                }
            }

            if (data.SrcTrans == 1)
            {
                data.SubTotal = totalDetail.Sum();
                data.FinalDisc = totalFinalDiscHeader.Sum();
                data.TaxAmount = totalTax.Sum();
                data.ExemptTaxAmount = totalExemptTax.Sum();
                data.Dpp = totalDpp.Sum();
                data.Total = data.SubTotal;
            }
 
            Db.SalesDeliveryHeaders.Add(data);

            var newInvCode = "";
            if (data.IsSoInv)
            {
                // Sales Invoice
                newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                var newSinvData = new SalesInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.InvDate,
                    DueDate = data.InvDueDate,
                    SoCode = data.TransCode,
                    CustCode = data.CustCode,
                    CurrCode = data.CurrCode,
                    Total = data.Total,
                    Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesInvoiceHeaders.Add(newSinvData);

                Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                {
                    Code = newInvCode,
                    LineNo = 1,
                    DoCode = newCode,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });
            }

            // Save changes
            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_do
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                data.Code, data.Date, data.TransCode);

            if (data.IsSoInv)
            {
                // Execute sp_update_stock_mutation_from_si
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                    newInvCode, data.Date, data.Code);
            }

            // Execute sp_update_so_dlv_qty /sp_update_sr_rcv_qty
            Db.Database.ExecuteSqlRaw(
                data.SrcTrans == 1 ? "EXEC sp_update_so_dlv_qty {0}" : "EXEC sp_update_sr_dlv_qty {0}",
                data.TransCode);

            if (data.IsSoInv)
            {
                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw("UPDATE Sales.SalesDeliveryHeader SET Mark='INV' WHERE Code={0}", data.Code);

                // Update sales order to closed if all sales delivery are invoiced
                //if (
                //    !Db.SalesDeliveryHeaders
                //        .Any(x => x.TransCode == data.TransCode && x.Mark != "INV"))
                //{
                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.TransCode);
                //}
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
        result.Message = "Data pengiriman penjualan berhasil disimpan.";
        return result;
    }

    public SaveResult Update(SalesDeliveryRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking mark header data
            if (Db.SalesDeliveryHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data pengiriman penjualan tidak bisa diubah karena data sudah ditandai sebagai void.";
                return result;
            }

            if (data.SrcTrans == 2)
            {
                var transData = Db.SalesReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan tidak ditemukan.";
                    return result;
                }

                // Checking sales return mark
                if (transData.Mark == "V")
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan sudah ditandai sebagai void.";
                    return result;
                }

                // Checking sales return date with sales delivery
                if (transData.Date > data.Date)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data retur penjualan mempunyai tanggal lebih besar.";
                    return result;
                }
            }
            else
            {
                var transData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan tidak ditemukan.";
                    return result;
                }

                // Checking sales order mark
                if (transData.Mark is "V" or "CLS")
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking sales order date with sales delivery
                if (transData.Date > data.Date)
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan mempunyai tanggal lebih besar.";
                    return result;
                }

                if (IsSaveAndDateInvalid(data))
                {
                    result.Message = "Tanggal Faktur Penjualan tidak boleh lebih kecil dari tanggal Surat Jalan.";
                    return result;
                }
            }

            // Checking warehouse qty is item is available or not
            var isQtyAvailable = IsQtyAvailable(data.Code, data.WarehouseCode, data.ItemDetails);
            switch (isQtyAvailable)
            {
                case 1:
                    result.Message = "Terdapat barang yang tidak tersedia pada gudang yang dipilih.";
                    return result;
                case 2:
                    result.Message = "Terdapat barang yang qty-nya melebihi ketersediaan pada gudang yang dipilih.";
                    return result;
            }

            // Checking deliver qty is excess or not
            if (IsQtyExcess(data.Code, data.SrcTrans, data.TransCode, data.ItemDetails))
            {
                result.Message = "Data pengiriman penjualan tidak bisa disimpan karena qty yg dikirim lebih besar dari qty yang tersedia.";
                return result;
            }

            //update Data if changed TransCode
            var oldDlvData = Db.SalesDeliveryHeaders.AsNoTracking().FirstOrDefault(x => x.Code == data.Code);
            if (oldDlvData.TransCode != data.TransCode)
            {
                RestoreDeliveredQty(oldDlvData.Code, oldDlvData.TransCode, oldDlvData.SrcTrans);
            }

            //Restore stock mutation
            RestoreWarehouseQty(oldDlvData.Code, oldDlvData.TransCode, oldDlvData.SrcTrans);

            var taxes = Db.Taxes.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();
            List<decimal> totalFinalDiscHeader = new();

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            // Get detail data that exists in receive before
            var delDetails = Db.SalesDeliveryDetails
                .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Get detail data that exists in receive before
            Db.SalesDeliveryDetails.RemoveRange(delDetails);

            // Update detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                if (data.SrcTrans == 1)
                {
                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);

                    if (data.IncludeTax)
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.Rate / 100)));
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.ExemptRate / 100)));
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.Rate / 100);
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.ExemptRate / 100);
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                    }

                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalFinalDiscHeader.Add(item.Qty * item.FinalDiscHeader);
                    totalTax.Add(item.Qty * item.TaxAmount);
                    totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                    totalDpp.Add(item.Qty * item.Dpp);
                }

                if (item.Id <= 0)
                {
                    var deliveryDetail = new SalesDeliveryDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        SoDetailId = item.SoDetailId,
                        ItemId = item.ItemId,
                        Qty = item.Qty,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        FinalDiscHeader = item.FinalDiscHeader,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        ExemptTaxAmount = item.ExemptTaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    };
                    Db.SalesDeliveryDetails.Add(deliveryDetail);

                    Db.SaveChanges();
                    listIdDetail.Add(deliveryDetail.Id);
                }
                else
                {
                    item.LineNo = ++i;

                    Db.SalesDeliveryDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;

                    listIdDetail.Add(item.Id);
                }

                if (item.FreeItemDetails != null)
                {
                    var delFreeDetails = Db.SalesDeliveryDetailFreeGoods
                        .Where(d => d.Code == data.Code && d.DlvOrderDetailId == item.Id && !item.FreeItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesDeliveryDetailFreeGoods.RemoveRange(delFreeDetails);

                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            if (freeItem.Id < 0)
                            {
                                Db.SalesDeliveryDetailFreeGoods.Add(new SalesDeliveryDetailFreeGood
                                {
                                    Code = data.Code,
                                    DlvOrderDetailId = listIdDetail[i - 1],
                                    LineNo = ++f,
                                    PromoCode = freeItem.PromoCode,
                                    ItemId = freeItem.ItemId,
                                    UomId = freeItem.UomId,
                                    UnitId = freeItem.UnitId,
                                    Qty = freeItem.Qty,
                                    UnitPrice = freeItem.UnitPrice,
                                    CoaCode = freeItem.CoaCode
                                });
                            }
                            else
                            {
                                freeItem.LineNo = ++f;

                                Db.SalesDeliveryDetailFreeGoods.Update(freeItem);
                                Db.Entry(freeItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(freeItem).Property(e => e.Code).IsModified = false;
                            }
                        }
                    }
                }
            }

            if (data.SrcTrans == 1)
            {
                data.SubTotal = totalDetail.Sum();
                data.FinalDisc = totalFinalDiscHeader.Sum();
                data.TaxAmount = totalTax.Sum();
                data.ExemptTaxAmount = totalExemptTax.Sum();
                data.Dpp = totalDpp.Sum();
                data.Total = data.SubTotal;
            }
            // Update header data
            Db.SalesDeliveryHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            var newInvCode = "";
            if (data.IsSoInv)
            {
                // Sales Invoice
                newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                var newSinvData = new SalesInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.InvDate,
                    DueDate = data.InvDueDate,
                    SoCode = data.TransCode,
                    CustCode = data.CustCode,
                    CurrCode = data.CurrCode,
                    Total = data.Total,
                    Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesInvoiceHeaders.Add(newSinvData);

                Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                {
                    Code = newInvCode,
                    LineNo = 1,
                    DoCode = data.Code,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });
            }

            // Save changes
            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_do
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                data.Code, data.Date, data.TransCode);

            if (data.IsSoInv)
            {
                // Execute sp_update_stock_mutation_from_si
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                    newInvCode, data.Date, data.Code);
            }

            // Execute sp_update_so_dlv_qty / sp_update_sr_rcv_qty
            if (data.SrcTrans == 1)
            {
                // Execute sp_update_so_dlv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.TransCode);

                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_free_dlv_qty {0}", data.TransCode);
            }
            else
            {
                // Execute sp_update_sr_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", data.TransCode);
            }

            if (data.IsSoInv)
            {
                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw("UPDATE Sales.SalesDeliveryHeader SET Mark='INV' WHERE Code={0}", data.Code);

                // Check all sales delivery are invoiced
                //if (
                //    !Db.SalesDeliveryHeaders
                //        .Any(x => x.TransCode == data.TransCode && x.Mark != "INV"))
                //{
                //    // Update sales order to closed
                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.TransCode);
                //}
                //else
                //{
                //    // Update sales order to partial receive or completed
                //    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.TransCode && x.Qty > x.QtyDlv)
                //        ? "PS"
                //        : "CMP";

                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.TransCode);
                //}
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
        result.Message = "Data pengiriman penjualan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.SalesDeliveryHeaders.Find(code);
        if (data != null)
        {
            var related = GetRelatedTransactions(data.Code);
            if (related.Count > 0)
            {
                result.Message = "Data pengiriman penjualan tidak bisa ditandai sebagai void karena terdapat transaksi terkait.";
                return result;
            }

            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data pengiriman penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}, {3}",
                    data.Code, data.Date, data.TransCode, true);

                if (data.SrcTrans == 1)
                {
                    // Execute sp_update_so_dlv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.TransCode);

                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_free_dlv_qty {0}", data.TransCode);
                }
                else
                {
                    // Execute sp_update_sr_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", data.TransCode);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
        }

        result.Success = true;
        result.Message = "Data pengiriman penjualan berhasil ditandai sebagai void.";
        return result;
    }
        
    private bool IsQtyExcess(string code, int srcTrans, string transCode, IEnumerable<SalesDeliveryDetail> items)
    {
        var result = false;
        if (srcTrans == 1) // Sales Order
        {
            foreach (var item in items)
            {
                var dataSODetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                if (code == null)
                {
                    var availableStock = dataSODetail.Qty - dataSODetail.QtyDlv;
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
                else if (dataSODetail != null)
                {
                    var oldSDD = Db.SalesDeliveryDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                    var availableStock = dataSODetail.Qty - (dataSODetail.QtyDlv - oldSDD?.Qty ?? 0);
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
            }
        }
        else // Sales Return
        {
            foreach (var item in items)
            {
                var srData = Db.SalesReturnHeaders.FirstOrDefault(x => x.Code == transCode);
                var dataSRDetail = Db.SalesReturnDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                var dataSRXDetail = Db.SalesReturnDetailExchDiffItems.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                if (code == null)
                {
                    var availableStock = srData.Type == 2
                        ? dataSRDetail.Qty - dataSRDetail.QtyDlv
                        : dataSRXDetail.Qty - dataSRXDetail.QtyDlv;
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
                else if (srData.Type == 2 ? dataSRDetail != null : dataSRXDetail != null)
                {
                    var oldSDD = Db.SalesDeliveryDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                    var availableStock = (srData.Type == 2 ? dataSRDetail.Qty : dataSRXDetail.Qty) - ((srData.Type == 2 ? dataSRDetail.Qty : dataSRXDetail.Qty) - oldSDD?.Qty ?? 0);
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
            }
        }
        return result;
    }

    private int IsQtyAvailable(string code, string warehouseCode, IEnumerable<SalesDeliveryDetail> items)
    {
        var result = 0;
        foreach (var item in items)
        {
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
            var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == item.ItemId);
            if (stock != null)
            {
                if (code == null)
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > stock.QtyOnHand)
                        {
                            result = 2;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > stock.QtyOnHand)
                        {
                            result = 2;
                        }
                    }
                }
                else
                {
                    var oldStock = Db.StockMutations.FirstOrDefault(x => x.ItemId == item.ItemId && x.RefCode1 == code);
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > stock.QtyOnHand - (oldStock?.BaseQty ?? 0))
                        {
                            result = 2;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > stock.QtyOnHand - (oldStock?.BaseQty ?? 0))
                        {
                            result = 2;
                        }
                    }
                }
            }
            else
            {
                result = 1;
                return result;
            }
        }
        return result;
    }

    public IEnumerable<dynamic> GetFreeDetailData(string code)
    {
        var result = (from dc in Db.SalesDeliveryDetailFreeGoods
                join sdd in Db.SalesDeliveryDetails on dc.DlvOrderDetailId equals sdd.Id
                join sod in Db.SalesOrderDetails on sdd.SoDetailId equals sod.Id
                join soc in Db.SalesOrderDetailFreeGoods on sod.Id equals soc.OrderDetailId
                join i in Db.Items on dc.ItemId equals i.Id
                join u in Db.UoMConversions on dc.UnitId equals u.Id
                where dc.Code == code && soc.UnitId == dc.UnitId && soc.ItemId == dc.ItemId
                select new
                {
                    Id = dc.Id,
                    Code = dc.Code,
                    DlvOrderDetailId = dc.DlvOrderDetailId,
                    LineNo = dc.LineNo,
                    PromoCode = dc.PromoCode,
                    ItemId = dc.ItemId,
                    UomId = dc.UomId,
                    UnitId = dc.UnitId,
                    Qty = dc.Qty,
                    UnitPrice = dc.UnitPrice,
                    CoaCode = dc.CoaCode,
                    Initial = i.Initial,
                    Name = i.Name,
                    UnitName = u.UnitEquivalent,
                    OutstandingQty = soc.Qty - soc.QtyClosed,
                    OldQty = dc.Qty
                }).ToList();

        return result.OrderBy(x => x.LineNo);
    }

    private bool IsSaveAndDateInvalid(SalesDeliveryRequest data)
    {
        var result = false;

        if (data.IsSoInv && data.InvDate < data.Date)
            result = true;

        return result;
    }

    private void RestoreDeliveredQty(string code, string transCode, int srcTrans)
    {
        var detailDoData = Db.SalesDeliveryDetails.AsNoTracking().Where(x => x.Code == code).ToList();
        var detailDoFreeData = Db.SalesDeliveryDetailFreeGoods.AsNoTracking().Where(x => x.Code == code).ToList();
        if (srcTrans == 1)
        {
            var SoData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == transCode);
            var detailSoData = Db.SalesOrderDetails.Where(x => x.Code == transCode).ToList();
            var detailSoFreeData = Db.SalesOrderDetailFreeGoods.Where(x => x.Code == transCode).ToList();
            foreach (var item in detailSoData)
            {
                if (detailDoData.Where(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Any())
                    item.QtyDlv -= detailDoData.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Qty;
            }
            Db.SalesOrderDetails.UpdateRange(detailSoData);

            if (detailSoFreeData.Count > 0)
            {
                foreach (var item in detailSoFreeData)
                {
                    if (detailDoFreeData.Where(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Any())
                        item.QtyClosed -= detailDoFreeData.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Qty;
                }
                SoData.Mark = (detailSoData.Sum(x => x.QtyDlv) == 0 && detailSoFreeData.Sum(x => x.QtyClosed) == 0) ? "A" : 
                    (detailSoData.Sum(x => x.QtyDlv) == detailSoData.Sum(x => x.Qty) && detailSoFreeData.Sum(x => x.QtyClosed) == detailSoFreeData.Sum(x => x.Qty)) ? "CMP" : "PS";
            }
            else
            {
                SoData.Mark = detailSoData.Sum(x => x.QtyDlv) == 0 ? "A" : detailSoData.Sum(x => x.QtyDlv) == detailSoData.Sum(x => x.Qty) ? "CMP" : "PS";
            }

            Db.SalesOrderHeaders.Update(SoData);
        }
        else
        {
            var SrData = Db.SalesReturnHeaders.FirstOrDefault(x => x.Code == transCode);
            var detailSrData = Db.SalesReturnDetails.Where(x => x.Code == transCode).ToList();
            foreach (var item in detailSrData)
            {
                if (detailSrData.Where(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Any())
                    item.QtyDlv -= detailSrData.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Qty;
            }
            Db.SalesReturnDetails.UpdateRange(detailSrData);
            SrData.Mark = detailSrData.Sum(x => x.QtyDlv) == 0 ? "A" : detailSrData.Sum(x => x.QtyDlv) == detailSrData.Sum(x => x.Qty) ? "CMP" : "PR";
            Db.SalesReturnHeaders.Update(SrData);
        }
        Db.SaveChanges();
    }

    private void RestoreWarehouseQty(string code, string srcCode, short srcTrans)
    {
        var dlvSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code).ToList();
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
}