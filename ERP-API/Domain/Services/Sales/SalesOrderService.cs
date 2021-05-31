using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Services.Sales
{
    public class SalesOrderService : GeneralService<SalesOrderHeader>, ISalesOrderService
    {
        public SalesOrderService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSalesOrderHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SalesInitial.Contains(search) || x.CustName.Contains(search) ||
                        x.CurrCode == search);
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesOrderDetail> GetDetailData(string code, bool? fullReceived)
        {
            var data = Db.VwSalesOrderDetails.Where(x => x.Code == code);

            if (fullReceived.HasValue)
            {
                data = (bool) fullReceived
                    ? data.Where(x => x.Qty <= x.QtyDlv)
                    : data.Where(x => x.Qty > x.QtyDlv);
            }

            return data.OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var data = from dlv in Db.SalesDeliveryHeaders
                       where dlv.TransCode == code && dlv.Mark == "A"
                       select new { dlv.Code, dlv.Date, dlv.Mark };

            return data.ToDynamicList();
        }

        public SaveResult Insert(SalesOrderRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking deliver qty is excess or not
                if (IsQtyExcess(data.WarehouseCode, data.ItemDetails))
                {
                    result.Message = "Data order penjualan tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("SO_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.SalesOrderHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesOrderDetails.Add(new SalesOrderDetail
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
                    });
                }

                if (data.IsSoDlv)
                {
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        TransCode = newCode,
                        CustCode = data.CustCode,
                        WarehouseCode = data.WarehouseCode,
                        ShippedBy = data.SalesBy,
                        CurrCode = data.CurrCode,
                        Rate = data.Rate,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDiscPercent = data.FinalDiscPercent,
                        FinalDisc = data.FinalDisc,
                        IncludeTax = data.IncludeTax,
                        TaxAmount = data.TaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp,
                        Mark = data.Mark,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesDeliveryHeaders.Add(newSdlvData);

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
                }

                if (data.IsSoInv)
                {
                    // Sales Delivery
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        TransCode = newCode,
                        CustCode = data.CustCode,
                        WarehouseCode = data.WarehouseCode,
                        ShippedBy = data.SalesBy,
                        CurrCode = data.CurrCode,
                        Rate = data.Rate,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDiscPercent = data.FinalDiscPercent,
                        FinalDisc = data.FinalDisc,
                        IncludeTax = data.IncludeTax,
                        TaxAmount = data.TaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp,
                        Mark = "INV",
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesDeliveryHeaders.Add(newSdlvData);

                    // Sales Invoice
                    var newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                    var newSinvData = new SalesInvoiceHeader
                    {
                        Code = newInvCode,
                        Date = data.InvDate,
                        DueDate = data.InvDueDate,
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
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesInvoiceHeaders.Add(newSinvData);

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

                    Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                    {
                        Code = newInvCode,
                        LineNo = 1,
                        DoCode = newDlvCode,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDisc = data.FinalDisc,
                        TaxAmount = data.TaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp
                    });
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    data.Code, data.Date);

                if (data.IsSoDlv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);
                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        DlvData.Code, data.Date, newCode);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);
                }

                if (data.IsSoInv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);
                    // Execute sp_update_stock_mutation_from_rcv
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
            result.Message = "Data order penjualan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesOrderRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesOrderHeaders.Any(x => x.Code == data.Code && new[] { "V", "CLS" }.Contains(x.Mark)))
                {
                    result.Message = "Data order penjualan tidak bisa diubah karena sudah ditandai sebagai void atau closed.";
                    return result;
                }

                // Checking deliver qty is excess or not
                if (IsQtyExcess(data.WarehouseCode, data.ItemDetails))
                {
                    result.Message = "Data order penjualan tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Update header data
                Db.SalesOrderHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.SalesOrderDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.SalesOrderDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id == 0)
                    {
                        Db.SalesOrderDetails.Add(new SalesOrderDetail
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
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                if (data.IsSoDlv)
                {
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        TransCode = data.Code,
                        CustCode = data.CustCode,
                        WarehouseCode = data.WarehouseCode,
                        ShippedBy = data.SalesBy,
                        CurrCode = data.CurrCode,
                        Rate = data.Rate,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDiscPercent = data.FinalDiscPercent,
                        FinalDisc = data.FinalDisc,
                        IncludeTax = data.IncludeTax,
                        TaxAmount = data.TaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp,
                        Mark = data.Mark,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesDeliveryHeaders.Add(newSdlvData);

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
                }

                if (data.IsSoInv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.Where(x => x.TransCode == data.Code).ToList();
                    if(DlvData.Count == 0)
                    {
                        // Sales Delivery
                        var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                        var newSdlvData = new SalesDeliveryHeader
                        {
                            Code = newDlvCode,
                            Date = data.DlvDate,
                            TransCode = data.Code,
                            CustCode = data.CustCode,
                            WarehouseCode = data.WarehouseCode,
                            ShippedBy = data.SalesBy,
                            CurrCode = data.CurrCode,
                            Rate = data.Rate,
                            ShipmentFee = data.ShipmentFee,
                            HandlingFee = data.HandlingFee,
                            SubTotal = data.SubTotal,
                            FinalDiscPercent = data.FinalDiscPercent,
                            FinalDisc = data.FinalDisc,
                            IncludeTax = data.IncludeTax,
                            TaxAmount = data.TaxAmount,
                            Total = data.Total,
                            Dpp = data.Dpp,
                            Mark = "INV",
                            CreatedBy = data.CreatedBy,
                            CreatedDate = data.CreatedDate,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = data.UpdatedDate
                        };

                        Db.SalesDeliveryHeaders.Add(newSdlvData);

                        // Sales Invoice
                        var newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                        var newSinvData = new SalesInvoiceHeader
                        {
                            Code = newInvCode,
                            Date = data.InvDate,
                            DueDate = data.InvDueDate,
                            SoCode = data.Code,
                            CustCode = data.CustCode,
                            IssuedBy = data.SalesBy,
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

                        Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                        {
                            Code = newInvCode,
                            LineNo = 1,
                            DoCode = newDlvCode,
                            ShipmentFee = data.ShipmentFee,
                            HandlingFee = data.HandlingFee,
                            SubTotal = data.SubTotal,
                            FinalDisc = data.FinalDisc,
                            TaxAmount = data.TaxAmount,
                            Total = data.Total,
                            Dpp = data.Dpp
                        });
                    }
                    else
                    {
                        // Sales Invoice
                        var newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                        var newSinvData = new SalesInvoiceHeader
                        {
                            Code = newInvCode,
                            Date = data.InvDate,
                            DueDate = data.InvDueDate,
                            SoCode = data.Code,
                            CustCode = data.CustCode,
                            IssuedBy = data.SalesBy,
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

                        short j = 0;
                        foreach (var Dlvitem in DlvData)
                        {
                            Dlvitem.Date = data.DlvDate;
                            Dlvitem.Mark = "INV";
                            Dlvitem.UpdatedBy = data.UpdatedBy;
                            Dlvitem.UpdatedDate = data.UpdatedDate;

                            Db.SalesDeliveryHeaders.Update(Dlvitem);

                            Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                            {
                                Code = newInvCode,
                                LineNo = ++j,
                                DoCode = Dlvitem.Code,
                                ShipmentFee = Dlvitem.ShipmentFee,
                                HandlingFee = Dlvitem.HandlingFee,
                                SubTotal = Dlvitem.SubTotal,
                                FinalDisc = Dlvitem.FinalDisc,
                                TaxAmount = Dlvitem.TaxAmount,
                                Total = Dlvitem.Total,
                                Dpp = Dlvitem.Dpp
                            });
                        }
                    }
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    data.Code, data.Date);

                if (data.IsSoDlv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == data.Code);
                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        DlvData.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);
                }

                if (data.IsSoInv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == data.Code);
                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        DlvData.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);

                    // Check all sales delivery are invoiced
                    if (
                        !Db.SalesDeliveryHeaders
                            .Any(x => x.TransCode == data.Code && x.Mark != "INV"))
                    {
                        // Update sales order to closed
                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.Code);
                    }
                    else
                    {
                        // Update sales order to partial receive or completed
                        var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.Code && x.Qty > x.QtyDlv)
                            ? "PS"
                            : "CMP";

                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.Code);
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
            result.Message = "Data order penjualan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesOrderHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data order penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data order penjualan berhasil ditandai sebagai void.";
            return result;
        }

        public SaveResult Close(string code, int userId)
        {
            var result = new SaveResult(false);
            var data = Db.SalesOrderHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "CLS")
                {
                    result.Message = "Data order penjualan tidak bisa ditutup karena sudah ditutup.";
                    return result;
                }
                // Update header data
                data.Mark = "CLS";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;
                Db.SaveChanges();
            }
            result.Success = true;
            result.Message = "Data order penjualan berhasil ditutup.";
            return result;
        }

        private bool IsQtyExcess(string warehouseCode, IEnumerable<SalesOrderDetail> items)
        {
            var result = false;
            foreach (var item in items)
            {
                var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
                var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == item.ItemId);
                var stockMOO = Db.StockMutations.Where(x => x.WarehouseCode == warehouseCode && x.UomId == item.UomId && x.Type == "OO").Sum(x => x.BaseQty);
                var stockMOH = Db.StockMutations.Where(x => x.WarehouseCode == warehouseCode && x.UomId == item.UomId && x.Type == "OH").Sum(x => x.BaseQty);
                if (stock != null)
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
                else if (!stockMOH.Equals(null)) // check to stock mutation if warehouse quantites not available
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > (stockMOH - stockMOO))
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > (stockMOH - stockMOO))
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }
    }
}
