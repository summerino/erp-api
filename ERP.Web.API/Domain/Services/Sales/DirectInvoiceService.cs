using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class DirectInvoiceService : GeneralService<SalesInvoiceHeader>, IDirectInvoiceService
    {
        public DirectInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DirectInvoiceRequest FindByCode(string code)
        {
            var invData = Db.VwSalesInvoiceHeaders.FirstOrDefault(x => x.Code == code && x.FromDirectInvoice);

            if (invData == null)
                return null;

            var ordData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == invData.SoCode);

            return new DirectInvoiceRequest
            {
                Code = invData.Code,
                Date = invData.Date,
                DueDate = invData.DueDate,
                SoCode = invData.SoCode,
                CurrCode = invData.CurrCode,
                CustCode = invData.CustCode,
                IssuedBy = invData.IssuedBy,
                PaidAmount = invData.PaidAmount,
                Total = invData.Total,
                Notes = invData.Notes,
                FromDirectInvoice = invData.FromDirectInvoice,
                Mark = invData.Mark,
                CreatedBy = invData.CreatedBy,
                CreatedDate = invData.CreatedDate,
                UpdatedBy = invData.UpdatedBy,
                UpdatedDate = invData.UpdatedDate,
                ApprovedBy = invData.ApprovedBy,
                ApprovedDate = invData.ApprovedDate,
                CustName = invData.CustName,
                IssuedInitial = invData.IssuedInitial,
                CreatedInitial = invData.CreatedInitial,
                UpdatedInitial = invData.UpdatedInitial,
                ApprovedInitial = invData.ApprovedInitial,
                Status = invData.Status,
                PaymentTermId = ordData.PaymentTermId,
                SalesBy = ordData?.SalesBy ?? 0,
                WarehouseCode = ordData?.WarehouseCode,
                ShipmentFee = ordData?.ShipmentFee ?? 0m,
                HandlingFee = ordData?.HandlingFee ?? 0m,
                SubTotal = ordData?.SubTotal ?? 0m,
                FinalDiscPercent = ordData?.FinalDiscPercent ?? 0m,
                FinalDisc = ordData?.FinalDisc ?? 0m,
                IncludeTax = ordData?.IncludeTax ?? false,
                TaxAmount = ordData?.TaxAmount ?? 0m,
                Dpp = ordData?.Dpp ?? 0m
            };
        }
        public List<dynamic> GetRelatedTransactions(string code)
        {

            var data = (from h in Db.GeneralCashBankHeaders
                        join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                        where h.Mark == "A" && d.TransCode == code
                        select new
                        {
                            h.Code,
                            h.Date,
                            h.Amount
                        });

            return data.ToDynamicList();
        }
        public SaveResult Insert(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);
            List<long> idOrderDetail = new();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking deliver qty is excess or not
                if (IsQtyExcess(data.WarehouseCode, data.ItemDetails, null))
                {
                    result.Message = "Data penjualan langsung tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Sales Order
                var newCode = GetNewCode("DI_NUM_FMT", data.Date);
                Db.SalesOrderHeaders.Add(new SalesOrderHeader
                {
                    Code = newCode,
                    Date = data.Date,
                    CustCode = data.CustCode,
                    PaymentTermId = data.PaymentTermId,
                    SalesBy = data.SalesBy,
                    WarehouseCode = data.WarehouseCode,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    SubTotal = data.SubTotal,
                    FinalDiscPercent = data.FinalDiscPercent,
                    FinalDisc = data.FinalDisc,
                    IncludeTax = data.IncludeTax,
                    TaxAmount = data.TaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp,
                    Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate,
                    FromDirectInvoice = true
                });

                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    var orderDetail = new SalesOrderDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        QtyDlv = 0,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp,
                        Notes = item.Notes,
                        CoaInventory = item.CoaInventory,
                        CoaCogs = item.CoaCogs,
                        CoaSls = item.CoaSls,
                        CoaSlsDisc = item.CoaSlsDisc,
                        CoaSlsReturn = item.CoaSlsReturn
                    };

                    Db.SalesOrderDetails.Add(orderDetail);
                    idOrderDetail.Add(orderDetail.Id);

                    if (item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                    {
                        Db.SaveChanges();
                    }

                    if (item.DiscountItemDetails.Any())
                    {
                        short d = 0;
                        foreach (var discItem in item.DiscountItemDetails)
                        {
                            Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                            {
                                Code = newCode,
                                OrderDetailId = orderDetail.Id,
                                LineNo = ++d,
                                PromoCode = discItem.PromoCode,
                                PromoDetailId = discItem.PromoDetailId,
                                Name = discItem.Name,
                                IsPercentage = discItem.IsPercentage,
                                Value = discItem.Value,
                                Amount = discItem.Amount,
                                CoaCode = discItem.CoaCode
                            });
                        }
                        Db.SaveChanges();
                    }

                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            Db.SalesOrderDetailFreeGoods.Add(new SalesOrderDetailFreeGood
                            {
                                Code = newCode,
                                OrderDetailId = orderDetail.Id,
                                LineNo = ++f,
                                PromoCode = freeItem.PromoCode,
                                ItemId = freeItem.ItemId,
                                UomId = freeItem.UomId,
                                UnitId = freeItem.UnitId,
                                Qty = freeItem.Qty,
                                QtyClosed = freeItem.QtyClosed,
                                UnitPrice = freeItem.UnitPrice,
                                CoaCode = freeItem.CoaCode
                            });
                        }
                        Db.SaveChanges();
                    }
                }
                // Sales Delivery
                Db.SalesDeliveryHeaders.Add(new SalesDeliveryHeader
                {
                    Code = newCode,
                    Date = data.Date,
                    SrcTrans = 1,
                    TransCode = newCode,
                    CustCode = data.CustCode,
                    WarehouseCode = data.WarehouseCode,
                    ShippedBy = data.SalesBy,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    SubTotal = data.SubTotal,
                    FinalDiscPercent = data.FinalDiscPercent,
                    FinalDisc = data.FinalDisc,
                    IncludeTax = data.IncludeTax,
                    TaxAmount = data.TaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp,
                    Mark = "INV",
                    Notes = data.Notes,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate,
                    FromDirectInvoice = true
                });

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    var deliveryDetail = new SalesDeliveryDetail
                    {
                        Code = newCode,
                        LineNo = ++j,
                        SoDetailId = idOrderDetail[j - 1],
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    };

                    Db.SalesDeliveryDetails.Add(deliveryDetail);

                    if (item.FreeItemDetails.Any())
                    {
                        Db.SaveChanges();
                    }

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
                        Db.SaveChanges();
                    }
                }

                // Sales Invoice
                Db.SalesInvoiceHeaders.Add(new SalesInvoiceHeader
                {
                    Code = newCode,
                    Date = data.Date,
                    DueDate = data.DueDate,
                    SoCode = newCode,
                    CustCode = data.CustCode,
                    IssuedBy = data.SalesBy,
                    CurrCode = data.CurrCode,
                    Total = data.Total,
                    Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate,
                    FromDirectInvoice = true
                });

                Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                {
                    Code = newCode,
                    LineNo = 1,
                    DoCode = newCode,
                    SubTotal = data.SubTotal,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });

                Db.SaveChanges();

                var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);
                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    newCode, data.Date);

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    DlvData.Code, data.Date, newCode);

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);

                // Update sales order to closed if all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == newCode && x.Mark != "INV"))
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newCode);
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
            result.Message = "Data penjualan langsung berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);
            var listOrderIdDetail = new List<long>();
            var listDeliveryIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (Db.SalesInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                // Checking deliver qty is excess or not
                if (IsQtyExcess(data.WarehouseCode, data.ItemDetails, data.Code))
                {
                    result.Message = "Data penjualan langsung tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update Invoice header data
                Db.SalesInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                //Update Order header data
                var OrderData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == data.SoCode);
                OrderData.Date = data.Date;
                OrderData.CustCode = data.CustCode;
                OrderData.PaymentTermId = data.PaymentTermId;
                OrderData.SalesBy = data.SalesBy;
                OrderData.WarehouseCode = data.WarehouseCode;
                OrderData.CurrCode = data.CurrCode;
                OrderData.Rate = data.Rate;
                OrderData.SubTotal = data.SubTotal;
                OrderData.FinalDiscPercent = data.FinalDiscPercent;
                OrderData.FinalDisc = data.FinalDisc;
                OrderData.IncludeTax = data.IncludeTax;
                OrderData.TaxAmount = data.TaxAmount;
                OrderData.Total = data.Total;
                OrderData.Dpp = data.Dpp;
                OrderData.Notes = data.Notes;
                OrderData.Mark = data.Mark;
                OrderData.UpdatedBy = data.UpdatedBy;
                OrderData.UpdatedDate = data.UpdatedDate;
                Db.SalesOrderHeaders.Update(OrderData);
                Db.Entry(OrderData).Property(e => e.Code).IsModified = false;
                Db.Entry(OrderData).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(OrderData).Property(e => e.CreatedDate).IsModified = false;

                //update Delivery header data
                var invDetail = Db.SalesInvoiceDetails.FirstOrDefault(x => x.Code == data.Code);
                var DeliveryData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == invDetail.DoCode);
                DeliveryData.Date = data.Date;
                DeliveryData.TransCode = OrderData.Code;
                DeliveryData.CustCode = data.CustCode;
                DeliveryData.WarehouseCode = data.WarehouseCode;
                DeliveryData.ShippedBy = data.SalesBy;
                DeliveryData.CurrCode = data.CurrCode;
                DeliveryData.Rate = data.Rate;
                DeliveryData.SubTotal = data.SubTotal;
                DeliveryData.FinalDiscPercent = data.FinalDiscPercent;
                DeliveryData.FinalDisc = data.FinalDisc;
                DeliveryData.IncludeTax = data.IncludeTax;
                DeliveryData.TaxAmount = data.TaxAmount;
                DeliveryData.Total = data.Total;
                DeliveryData.Dpp = data.Dpp;
                DeliveryData.Mark = "INV";
                DeliveryData.Notes = data.Notes;
                DeliveryData.UpdatedBy = data.UpdatedBy;
                DeliveryData.UpdatedDate = data.UpdatedDate;
                Db.SalesDeliveryHeaders.Update(DeliveryData);
                Db.Entry(DeliveryData).Property(e => e.Code).IsModified = false;
                Db.Entry(DeliveryData).Property(e => e.SrcTrans).IsModified = false;
                Db.Entry(DeliveryData).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(DeliveryData).Property(e => e.CreatedDate).IsModified = false;

                //Update Invoice detail data
                var InvoiceDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.Code == data.Code);
                InvoiceDetailData.SubTotal = data.SubTotal;
                InvoiceDetailData.FinalDisc = data.FinalDisc;
                InvoiceDetailData.TaxAmount = data.TaxAmount;
                InvoiceDetailData.Total = data.Total;
                InvoiceDetailData.Dpp = data.Dpp;
                Db.SalesInvoiceDetails.Update(InvoiceDetailData);
                Db.Entry(InvoiceDetailData).Property(e => e.Code).IsModified = false;

                //Update Order Detail data
                    // Get detail data that exists in order before
                var delOrderDetails = Db.SalesOrderDetails
                    .Where(d => d.Code == data.SoCode && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                    // Delete detail data that exists in order before
                Db.SalesOrderDetails.RemoveRange(delOrderDetails);

                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id == 0)
                    {
                        var orderDetail = new SalesOrderDetail
                        {
                            Code = item.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            Length = item.Length,
                            Width = item.Width,
                            Height = item.Height,
                            Weight = item.Weight,
                            DimensionMeasurement = item.DimensionMeasurement,
                            WeightMeasurement = item.WeightMeasurement,
                            QtyDlv = 0,
                            UnitPrice = item.UnitPrice,
                            Disc = item.Disc,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp,
                            Notes = item.Notes,
                            CoaInventory = item.CoaInventory,
                            CoaCogs = item.CoaCogs,
                            CoaSls = item.CoaSls,
                            CoaSlsDisc = item.CoaSlsDisc,
                            CoaSlsReturn = item.CoaSlsReturn
                        };

                        Db.SalesOrderDetails.Add(orderDetail);

                        if (item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            Db.SaveChanges();
                            listOrderIdDetail.Add(orderDetail.Id);
                        }
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        if (item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            listOrderIdDetail.Add(item.Id);
                        }
                    }

                    var delDiscDetails = Db.SalesOrderDetailDiscounts
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !item.DiscountItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesOrderDetailDiscounts.RemoveRange(delDiscDetails);

                    if (item.DiscountItemDetails.Any())
                    {
                        short d = 0;
                        foreach (var discItem in item.DiscountItemDetails)
                        {
                            if (discItem.Id < 0)
                            {
                                Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++d,
                                    PromoCode = discItem.PromoCode,
                                    PromoDetailId = discItem.PromoDetailId,
                                    Name = discItem.Name,
                                    IsPercentage = discItem.IsPercentage,
                                    Value = discItem.Value,
                                    Amount = discItem.Amount,
                                    CoaCode = discItem.CoaCode
                                });
                            }
                            else
                            {
                                discItem.LineNo = ++d;

                                Db.SalesOrderDetailDiscounts.Update(discItem);
                                Db.Entry(discItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(discItem).Property(e => e.Code).IsModified = false;
                            }
                        }
                    }

                    var delFreeDetails = Db.SalesOrderDetailFreeGoods
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !item.FreeItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesOrderDetailFreeGoods.RemoveRange(delFreeDetails);

                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            if (freeItem.Id < 0)
                            {
                                Db.SalesOrderDetailFreeGoods.Add(new SalesOrderDetailFreeGood
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++f,
                                    PromoCode = freeItem.PromoCode,
                                    ItemId = freeItem.ItemId,
                                    UomId = freeItem.UomId,
                                    UnitId = freeItem.UnitId,
                                    Qty = freeItem.Qty,
                                    QtyClosed = freeItem.QtyClosed,
                                    UnitPrice = freeItem.UnitPrice,
                                    CoaCode = freeItem.CoaCode
                                });
                            }
                            else
                            {
                                freeItem.LineNo = ++f;

                                Db.SalesOrderDetailFreeGoods.Update(freeItem);
                                Db.Entry(freeItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(freeItem).Property(e => e.Code).IsModified = false;
                            }
                        }
                    }
                }

                //Update Delivery detail data
                    // Get detail data that exists in order before
                var delDetails = Db.SalesDeliveryDetails
                    .Where(d => d.Code == InvoiceDetailData.DoCode)
                    .ToList();

                    // Get detail data that exists in receive before
                Db.SalesDeliveryDetails.RemoveRange(delDetails);

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    var deliveryDetail = new SalesDeliveryDetail
                    {
                        Code = InvoiceDetailData.DoCode,
                        LineNo = ++j,
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    };
                    Db.SalesDeliveryDetails.Add(deliveryDetail);

                    if (item.FreeItemDetails.Any())
                    {
                        Db.SaveChanges();
                        listDeliveryIdDetail.Add(deliveryDetail.Id);
                    }

                    var delFreeDetails = Db.SalesDeliveryDetailFreeGoods
                        .Where(d => d.Code == data.Code && d.DlvOrderDetailId == item.Id && !item.FreeItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesDeliveryDetailFreeGoods.RemoveRange(delFreeDetails);

                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            Db.SalesDeliveryDetailFreeGoods.Add(new SalesDeliveryDetailFreeGood
                            {
                                Code = InvoiceDetailData.DoCode,
                                DlvOrderDetailId = listDeliveryIdDetail[j - 1],
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
                        Db.SaveChanges();
                    }
                }


                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    data.SoCode, data.Date);

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    InvoiceDetailData.DoCode, data.Date, data.SoCode);

                // Check all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
                {
                    // Update sales order to closed
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
                }
                else
                {
                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);
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
            result.Message = "Data penjualan langsung berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            if (IsAlreadyInTransaction(code)) 
            {
                result.Message = "Data faktur penjualan tidak bisa ditandai sebagai void karena sudah ada ditransaksi.";
                return result;
            }

            var data = Db.SalesInvoiceHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data faktur penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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

                    // Get sales delivery code and join
                    var doCodeJoin = string.Join("','",
                        Db.SalesInvoiceDetails
                            .Where(x => x.Code == code)
                            .Select(x => x.DoCode.Replace("'", "''")));

                    // Update sales delivery to active
                    Db.Database.ExecuteSqlRaw(
                        $"UPDATE Sales.SalesDeliveryHeader SET Mark='A' WHERE Code IN ('{doCodeJoin}')");

                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data penjualan langsung berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsAlreadyInTransaction(string code) 
        {
            return (from h in Db.GeneralCashBankHeaders
                    join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                    where h.Mark == "A" && d.TransCode == code
                    select h.Code).Any();
        }

        private bool IsQtyExcess(string warehouseCode, IEnumerable<SalesOrderDetail> items, string code)
        {
            var result = false;
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
                            if (item.Qty > (stock.QtyOnHand - stock.QtyOnOrder))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                            var baseQty = item.Qty * multipliedQty;
                            if (baseQty > (stock.QtyOnHand - stock.QtyOnOrder))
                            {
                                result = true;
                            }
                        }
                    }
                    else
                    {
                        var oldStock = Db.StockMutations.FirstOrDefault(x => x.ItemId == item.ItemId && x.RefCode1 == code);
                        if (uom.IsBaseUnit)
                        {
                            if (item.Qty > (stock.QtyOnHand - (stock.QtyOnOrder - oldStock.BaseQty)))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                            var baseQty = item.Qty * multipliedQty;
                            if (baseQty > (stock.QtyOnHand - (stock.QtyOnOrder - oldStock.BaseQty)))
                            {
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
