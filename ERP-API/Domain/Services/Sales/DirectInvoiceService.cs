using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Models.Sales;
using ERP_API.Model.Sales;
using System.Collections.Generic;

namespace ERP_API.Domain.Services.Sales
{
    public class DirectInvoiceService : GeneralService<SalesInvoiceHeader>, IDirectInvoiceService
    {
        public DirectInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DirectInvoiceHeader FindByCode(string code)
        {
            var invData = Db.VwSalesInvoiceHeaders.FirstOrDefault(x => x.Code == code && x.FromDirectInvoice);

            if (invData == null)
                return null;

            var ordData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == invData.SoCode);

            return new DirectInvoiceHeader
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
        
        public SaveResult Insert(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Sales Order
                var newOrderCode = GetNewCode("SO_NUM_FMT", data.Date);
                Db.SalesOrderHeaders.Add(new SalesOrderHeader
                {
                    Code = newOrderCode,
                    Date = data.Date,
                    CustCode = data.CustCode,
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
                    Db.SalesOrderDetails.Add(new SalesOrderDetail
                    {
                        Code = newOrderCode,
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
                    });
                }
                // Sales Delivery
                var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                Db.SalesDeliveryHeaders.Add(new SalesDeliveryHeader
                {
                    Code = newDlvCode,
                    Date = data.Date,
                    TransCode = newOrderCode,
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
                    Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                    {
                        Code = newDlvCode,
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
                    });
                }

                // Sales Invoice
                var newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                Db.SalesInvoiceHeaders.Add(new SalesInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.Date,
                    DueDate = data.DueDate,
                    SoCode = newOrderCode,
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
                    Code = newInvCode,
                    LineNo = 1,
                    DoCode = newDlvCode,
                    SubTotal = data.SubTotal,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });

                Db.SaveChanges();

                var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newOrderCode);
                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    DlvData.Code, data.Date, newOrderCode);

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newOrderCode);

                // Update sales order to closed if all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == newOrderCode && x.Mark != "INV"))
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newOrderCode);
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

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (Db.SalesInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                Db.SalesInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get delivery code that exists in invoice before
                var doCodeList = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.DoCode).Contains(d.DoCode))
                    .Select(x => x.DoCode)
                    .ToList();

                // Update sales delivery mark that exists in invoice before
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='A'
                    WHERE Code IN ('{string.Join("','", doCodeList)}')");

                // Get detail data that exists in invoice before
                var delDetails = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in invoice before
                Db.SalesInvoiceDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                var newInvDetails = new List<SalesInvoiceDetail>();
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        newInvDetails.Add(new SalesInvoiceDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            DoCode = item.DoCode,
                            ShipmentFee = item.ShipmentFee,
                            HandlingFee = item.HandlingFee,
                            SubTotal = item.SubTotal,
                            FinalDisc = item.FinalDisc,
                            TaxAmount = item.TaxAmount,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesInvoiceDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Insert detail if new data exists
                if (newInvDetails.Any())
                    Db.SalesInvoiceDetails.AddRange(newInvDetails);

                // Save changes
                Db.SaveChanges();

                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.DoCode.Replace("'", "''")))}')");

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
            result.Message = "Data penjualan langsung berhasil diubah.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

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
    }
}
