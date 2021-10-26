using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Domain.Models.Mobile.Purchase;
using ERP.Web.API.Model.Purchase;
using Swift.Framework;
using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class PurchaseOrderService : GeneralService<PurchaseOrderHeader>, IPurchaseOrderService
    {
        public PurchaseOrderService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwPurchaseOrderHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.RequestInitial.Contains(search) || x.SupName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwPurchaseOrderDetail> GetDetailData(string code, bool? fullReceived)
        {
            var data = Db.VwPurchaseOrderDetails.Where(x => x.Code == code);

            if (fullReceived.HasValue)
            {
                data = (bool) fullReceived
                    ? data.Where(x => x.Qty <= x.QtyRcv)
                    : data.Where(x => x.Qty > x.QtyRcv);
            }

            return data.OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var data = from pr in Db.PurchaseReceiveHeaders
                       where pr.TransCode == code && pr.Mark != "V"
                       select new { pr.Code, pr.Date, pr.Mark };

            return data.ToDynamicList();
        }

        public IEnumerable<VwPurchaseOrderHeader> GetInCompleteInvoiceData(string searchBy, string search, string invCode)
        {
            var data = Db.VwPurchaseOrderHeaders.Where(x => new[] { "PR", "CMP" }.Contains(x.Mark));

            if (!string.IsNullOrEmpty(search))
            {
                data = searchBy switch
                {
                    "code" => data.Where(x => x.Code.Contains(search)),
                    "date" => data.Where(x => x.Date == Convert.ToDateTime(search)),
                    "supName" => data.Where(x => x.SupName.Contains(search)),
                    _ => data
                };
            }

            data = string.IsNullOrWhiteSpace(invCode)
                ? data.Where(x => Db.PurchaseReceiveHeaders
                                    .Where(r => r.Mark == "A" && r.SrcTrans == 1)
                                    .Select(r => r.TransCode).Contains(x.Code))
                : data.Where(x => Db.PurchaseReceiveHeaders
                                      .Where(r => r.Mark == "A" && r.SrcTrans == 1)
                                      .Select(r => r.TransCode).Contains(x.Code) ||
                                  Db.PurchaseInvoiceHeaders
                                      .Where(i => i.Code == invCode)
                                      .Select(i => i.PoCode).Contains(x.Code));

            return searchBy switch
            {
                "code" => data.OrderBy(x => x.Code),
                "date" => data.OrderBy(x => x.Date),
                "supName" => data.OrderBy(x => x.SupName),
                _ => data
            };
        }

        public SaveResult Insert(PurchaseOrderRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("PO_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.PurchaseOrderHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    var orderDetail = new PurchaseOrderDetail
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
                        QtyRcv = 0,
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
                        CoaPurc = item.CoaPurc,
                        CoaPurcDisc = item.CoaPurcDisc,
                        CoaPurcReturn = item.CoaPurcReturn,
                        Type = 0
                    };

                    Db.PurchaseOrderDetails.Add(orderDetail);

                    if (data.IsPoInv || data.IsPoRcv)
                    {
                        Db.SaveChanges();
                        listIdDetail.Add(orderDetail.Id);
                    }
                }

                if (data.IsPoRcv)
                {
                    var newRcvCode = GetNewCode("RCV_NUM_FMT", data.RcvDate);

                    var newPrcvData = new PurchaseReceiveHeader
                    {
                        Code = newRcvCode,
                        Date = data.RcvDate,
                        TransCode = newCode,
                        RefNo = data.RcvRefNo,
                        SrcTrans = 1,
                        SupCode = data.SupCode,
                        ReceiveBy = data.RequestBy,
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

                    Db.PurchaseReceiveHeaders.Add(newPrcvData);

                    short j = 0;
                    foreach (var item in data.ItemDetails)
                    {
                        Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                        {
                            Code = newRcvCode,
                            LineNo = ++j,
                            TransDetailId = listIdDetail[j-1],
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
                            Dpp = item.Dpp,
                            WarehouseCode = data.WarehouseCode,
                            Type = 0
                        });
                    }
                }

                if (data.IsPoInv)
                {

                    // Purchase Receive
                    var newRcvCode = GetNewCode("RCV_NUM_FMT", data.RcvDate);
                    var newPrcvData = new PurchaseReceiveHeader
                    {
                        Code = newRcvCode,
                        Date = data.RcvDate,
                        TransCode = newCode,
                        RefNo = data.RcvRefNo,
                        SrcTrans = 1,
                        SupCode = data.SupCode,
                        ReceiveBy = data.RequestBy,
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

                    Db.PurchaseReceiveHeaders.Add(newPrcvData);

                    // Purchase Invoice
                    var newInvCode = GetNewCode("PI_NUM_FMT", data.Date);
                    var newPinvData = new PurchaseInvoiceHeader
                    {
                        Code = newInvCode,
                        Date = data.InvDate,
                        DueDate = data.InvDueDate,
                        PoCode = newCode,
                        RefNo = data.InvRefNo,
                        SupCode = data.SupCode,
                        IssuedBy = data.RequestBy,
                        CurrCode = data.CurrCode,
                        PaidAmount = data.Total,
                        Total = data.Total,
                        Notes = data.Notes,
                        Mark = data.Mark,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.PurchaseInvoiceHeaders.Add(newPinvData);

                    short j = 0;
                    foreach (var item in data.ItemDetails)
                    {
                        Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                        {
                            Code = newRcvCode,
                            LineNo = ++j,
                            TransDetailId = listIdDetail[j-1],
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
                            Dpp = item.Dpp,
                            WarehouseCode = data.WarehouseCode,
                            Type = 0
                        });
                    }

                    Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                    {
                        Code = newInvCode,
                        LineNo = 1,
                        RcvCode = newRcvCode,
                        SubTotal = data.Total,
                        FinalDisc = data.FinalDisc,
                        TaxAmount = data.TaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp
                    });
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_po
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_po {0}, {1}",
                    data.Code, data.Date);

                if (data.IsPoRcv)
                {
                    var RcvData = Db.PurchaseReceiveHeaders.FirstOrDefault(x => x.TransCode == newCode);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                        RcvData.Code, data.Date, newCode);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", newCode);
                }

                if (data.IsPoInv)
                {
                    var RcvData = Db.PurchaseReceiveHeaders.FirstOrDefault(x => x.TransCode == newCode);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                        RcvData.Code, data.Date, newCode);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", newCode);

                    // Update purchase order to closed if all purchase receive are invoiced
                    if (
                        !Db.PurchaseReceiveHeaders
                            .Any(x => x.TransCode == newCode && x.Mark != "INV"))
                    {
                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newCode);
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
            result.Message = "Data order pembelian berhasil disimpan.";
            return result;
        }

        public SaveResult Update(PurchaseOrderRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.PurchaseOrderHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data order pembelian tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.PurchaseOrderHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.PurchaseOrderDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.PurchaseOrderDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        var orderDetail = new PurchaseOrderDetail
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
                            QtyRcv = 0,
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
                            CoaPurc = item.CoaPurc,
                            CoaPurcDisc = item.CoaPurcDisc,
                            CoaPurcReturn = item.CoaPurcReturn,
                            Type = 0
                        };

                        Db.PurchaseOrderDetails.Add(orderDetail);

                        if (data.IsPoInv || data.IsPoRcv)
                        {
                            Db.SaveChanges();
                            listIdDetail.Add(orderDetail.Id);
                        }
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PurchaseOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        if (data.IsPoInv || data.IsPoRcv)
                        {
                            listIdDetail.Add(item.Id);
                        }
                    }
                }

                if (data.IsPoRcv)
                {
                    var newRcvCode = GetNewCode("RCV_NUM_FMT", data.RcvDate);

                    var newPrcvData = new PurchaseReceiveHeader
                    {
                        Code = newRcvCode,
                        Date = data.RcvDate,
                        TransCode = data.Code,
                        RefNo = data.RcvRefNo,
                        SrcTrans = 1,
                        SupCode = data.SupCode,
                        ReceiveBy = data.RequestBy,
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

                    Db.PurchaseReceiveHeaders.Add(newPrcvData);

                    short j = 0;
                    foreach (var item in data.ItemDetails)
                    {
                        Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                        {
                            Code = newRcvCode,
                            LineNo = ++j,
                            TransDetailId = listIdDetail[j-1],
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
                            Dpp = item.Dpp,
                            WarehouseCode = data.WarehouseCode,
                            Type = 0
                        });
                    }
                }

                if (data.IsPoInv)
                {
                    // Purchase Receive
                    var RcvData = Db.PurchaseReceiveHeaders.Where(x => x.TransCode == data.Code).ToList();
                    if (RcvData.Count == 0)
                    {
                        var newRcvCode = GetNewCode("RCV_NUM_FMT", data.RcvDate);
                        var newPrcvData = new PurchaseReceiveHeader
                        {
                            Code = newRcvCode,
                            Date = data.RcvDate,
                            TransCode = data.Code,
                            RefNo = data.RcvRefNo,
                            SrcTrans = 1,
                            SupCode = data.SupCode,
                            ReceiveBy = data.RequestBy,
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

                        Db.PurchaseReceiveHeaders.Add(newPrcvData);

                        // Purchase Invoice
                        var newInvCode = GetNewCode("PI_NUM_FMT", data.Date);
                        var newPinvData = new PurchaseInvoiceHeader
                        {
                            Code = newInvCode,
                            Date = data.InvDate,
                            DueDate = data.InvDueDate,
                            PoCode = data.Code,
                            RefNo = data.InvRefNo,
                            SupCode = data.SupCode,
                            IssuedBy = data.RequestBy,
                            CurrCode = data.CurrCode,
                            PaidAmount = data.Total,
                            Total = data.Total,
                            Notes = data.Notes,
                            Mark = data.Mark,
                            CreatedBy = data.CreatedBy,
                            CreatedDate = data.CreatedDate,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = data.UpdatedDate
                        };

                        Db.PurchaseInvoiceHeaders.Add(newPinvData);

                        short j = 0;
                        foreach (var item in data.ItemDetails)
                        {
                            Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                            {
                                Code = newRcvCode,
                                LineNo = ++j,
                                TransDetailId = listIdDetail[j - 1],
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
                                Dpp = item.Dpp,
                                WarehouseCode = data.WarehouseCode,
                                Type = 0
                            });
                        }

                        Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                        {
                            Code = newInvCode,
                            LineNo = 1,
                            RcvCode = newRcvCode,
                            SubTotal = data.Total,
                            FinalDisc = data.FinalDisc,
                            TaxAmount = data.TaxAmount,
                            Total = data.Total,
                            Dpp = data.Dpp
                        });
                    }
                    else
                    {
                        // Purchase Invoice
                        var newInvCode = GetNewCode("PI_NUM_FMT", data.Date);
                        var newPinvData = new PurchaseInvoiceHeader
                        {
                            Code = newInvCode,
                            Date = data.InvDate,
                            DueDate = data.InvDueDate,
                            PoCode = data.Code,
                            RefNo = data.InvRefNo,
                            SupCode = data.SupCode,
                            IssuedBy = data.RequestBy,
                            CurrCode = data.CurrCode,
                            PaidAmount = data.Total,
                            Total = data.Total,
                            Notes = data.Notes,
                            Mark = data.Mark,
                            CreatedBy = data.CreatedBy,
                            CreatedDate = data.CreatedDate,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = data.UpdatedDate
                        };

                        Db.PurchaseInvoiceHeaders.Add(newPinvData);

                        short j = 0;
                        foreach (var RcvItem in RcvData)
                        {
                            RcvItem.Date = data.RcvDate;
                            RcvItem.RefNo = data.RcvRefNo;
                            RcvItem.Mark = "INV";
                            RcvItem.UpdatedBy = data.UpdatedBy;
                            RcvItem.UpdatedDate = data.UpdatedDate;

                            Db.PurchaseReceiveHeaders.Update(RcvItem);

                            Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                            {
                                Code = newInvCode,
                                LineNo = ++j,
                                RcvCode = RcvItem.Code,
                                SubTotal = RcvItem.Total,
                                FinalDisc = RcvItem.FinalDisc,
                                TaxAmount = RcvItem.TaxAmount,
                                Total = RcvItem.Total,
                                Dpp = RcvItem.Dpp
                            });
                        }                        
                    }
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_po
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_po {0}, {1}",
                    data.Code, data.Date);

                if (data.IsPoRcv)
                {
                    var RcvData = Db.PurchaseReceiveHeaders.FirstOrDefault(x => x.TransCode == data.Code);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                        RcvData.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.Code);
                }

                if (data.IsPoInv)
                {
                    var RcvData = Db.PurchaseReceiveHeaders.FirstOrDefault(x => x.TransCode == data.Code);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                        RcvData.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.Code);

                    // Check all purchase receive are invoiced
                    if (
                        !Db.PurchaseReceiveHeaders
                            .Any(x => x.TransCode == data.Code && x.Mark != "INV"))
                    {
                        // Update purchase order to closed
                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.Code);
                    }
                    else
                    {
                        // Update purchase order to partial receive or completed
                        var poMark = Db.PurchaseOrderDetails.Any(x => x.Code == data.Code && x.Qty > x.QtyRcv)
                            ? "PR"
                            : "CMP";

                        Db.Database.ExecuteSqlRaw(
                            "UPDATE Purchasing.PurchaseOrderHeader SET Mark={0} WHERE Code={1}", poMark, data.Code);
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
            result.Message = "Data order pembelian berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PurchaseOrderHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data order pembelian tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_po
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_po {0}, {1}, {2}",
                    data.Code, data.Date, true);
            }

            result.Success = true;
            result.Message = "Data order pembelian berhasil ditandai sebagai void.";
            return result;
        }

        public SaveResult Close(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PurchaseOrderHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "CLS")
                {
                    result.Message = "Data order pembelian tidak bisa ditutup karena sudah ditutup.";
                    return result;
                }

                // Update header data
                data.Mark = "CLS";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data order pembelian berhasil ditutup.";
            return result;
        }

        #region Mobile
        public DataSourceResult GetDataForMobile(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var dataOrder = (from order in Db.PurchaseOrderHeaders
                             join sup in Db.Suppliers on order.SupCode equals sup.Code
                             join supType in Db.SupplierTypes on sup.TypeId equals supType.Id
                             select new PurchaseOrderHeaderModel
                             {
                                 Code = order.Code,
                                 Date = order.Date,
                                 SupCode = order.SupCode,
                                 SupName = sup.Name,
                                 SupPhone = sup.Phone,
                                 SupTypeId = sup.TypeId,
                                 SupTypeName = supType.Name,
                                 Mark = order.Mark,
                                 WarehouseCode = order.WarehouseCode
                             }).AsQueryable();

            dataOrder = dataOrder.Where(x => x.Mark != "CMP" && x.Mark != "CLS" && x.Mark != "V");

            var dataRetur = (from retur in Db.PurchaseReturnHeaders
                             join returDetail in Db.PurchaseReturnDetails on retur.Code equals returDetail.Code
                             join sup in Db.Suppliers on retur.SupCode equals sup.Code
                             join supType in Db.SupplierTypes on sup.TypeId equals supType.Id
                             select new PurchaseOrderHeaderModel
                             {
                                 Code = retur.Code,
                                 Date = retur.Date,
                                 SupCode = retur.SupCode,
                                 SupName = sup.Name,
                                 SupPhone = sup.Phone,
                                 SupTypeId = sup.TypeId,
                                 SupTypeName = supType.Name,
                                 Mark = retur.Mark,
                                 WarehouseCode = returDetail.WarehouseCode
                             }).AsQueryable();

            var data = dataOrder.Union(dataRetur);

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search) || x.SupName.Contains(search));
            }

            if (date != null && date != "")
            {
                var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                data = data.Where(x => x.Date.Equals(date1));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<PurchaseOrderDetailModel> GetDetailDataForMobile(string code, int srcTrans)
        {
            if (srcTrans == 1)
            {
                var data = from order in Db.VwPurchaseOrderDetails
                           join header in Db.VwPurchaseOrderHeaders on order.Code equals header.Code
                           join item in Db.Items on order.ItemId equals item.Id
                           where order.Code.Equals(code) && order.Qty <= order.QtyRcv
                           select new PurchaseOrderDetailModel
                           {
                               Code = order.Code,
                               LineNo = order.LineNo,
                               ItemId = order.ItemId,
                               OrderQty = order.Qty,
                               ReceiveQty = order.QtyRcv,
                               TransDetailId = 2,
                               Type = 1,
                               UnitId = order.UnitId,
                               UomId = order.UomId,
                               WarehouseCode = header.WarehouseCode,

                               ItemInitial = item.Initial,
                               ItemName = order.ItemName,
                               UnitName = order.UnitName
                           };
                return data;
            }
            else
            {
                var data = Db.VwPurchaseReturnDetails.Where(x => x.Code.Equals(code) && x.Qty <= x.QtyRcv)
                    .Join(Db.Items, retur => retur.ItemId, item => item.Id, (retur, item) => new PurchaseOrderDetailModel
                    {
                        Code = retur.Code,
                        LineNo = retur.LineNo,
                        ItemId = retur.ItemId,
                        OrderQty = retur.Qty,
                        ReceiveQty = retur.QtyRcv,
                        TransDetailId = 1,
                        Type = 1,
                        UnitId = retur.UnitId,
                        UomId = retur.UomId,
                        WarehouseCode = retur.WarehouseCode,

                        ItemInitial = item.Initial,
                        ItemName = retur.ItemName,
                        UnitName = retur.UnitName
                    });

                return data;
            }
        }

        public DataSourceResult GetLogDataForMobile(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = from rcvHeader in Db.MobileReceiveItemHeaders
                       join po in Db.VwPurchaseOrderHeaders on rcvHeader.TransCode equals po.Code
                       join sup in Db.VwSuppliers on rcvHeader.SupCode equals sup.Code
                       select new ReceiveItemHeaderModel
                       {
                           Code = rcvHeader.Code,
                           Date = rcvHeader.Date,
                           PODate = po.Date,
                           PONumber = po.Code,
                           RcvCode = rcvHeader.RcvCode,
                           ReceiveBy = rcvHeader.ReceiveBy,
                           SrcTrans = rcvHeader.SrcTrans,
                           SupCode = rcvHeader.SupCode,
                           SupName = sup.Name,
                           SupPhone = sup.Phone,
                           TransCode = rcvHeader.TransCode
                       };

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search) || x.SupName.Contains(search));
            }

            if (date != null && date != "")
            {
                var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                data = data.Where(x => x.Date.Equals(date1));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ReceiveItemDetailModel> GetLogDetailDataForMobile(string code)
        {
            var data = from detail in Db.MobileReceiveItemDetails
                       join item in Db.Items on detail.ItemId equals item.Id
                       join uom in Db.UoMConversions on detail.UnitId equals uom.Id
                       join header in Db.MobileReceiveItemHeaders on detail.Code equals header.Code
                       join orderHeader in Db.PurchaseOrderHeaders on header.TransCode equals orderHeader.Code
                       join orderDetail in Db.PurchaseOrderDetails on orderHeader.Code equals orderDetail.Code
                       where detail.Code.Equals(code)
                       select new ReceiveItemDetailModel
                       {
                           Code = detail.Code,
                           Id = detail.Id,
                           ItemId = detail.ItemId,
                           LineNo = detail.LineNo,
                           Qty = detail.Qty,
                           TransDetailId = detail.TransDetailId,
                           Type = detail.Type,
                           UnitId = detail.UnitId,
                           UomId = detail.UomId,
                           UnitEquivalent = uom.UnitEquivalent,
                           WarehouseCode = detail.WarehouseCode,
                           ItemInitial = item.Initial,
                           ItemName = item.Name,
                           QtyOrder = orderDetail.Qty,
                           QtyRemain = orderDetail.Qty - detail.Qty
                       };
            return data;
        }

        public SaveResult InsertForMobile(PurchaseOrderRequestModel data, int UserId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var newCode = GetNewCode("RCV_NUM_FMT", data.Date);

                data.Code = newCode;

                Db.MobileReceiveItemHeaders.Add(data);

                short i = 0;
                foreach (var rcv in data.PODetails)
                {
                    rcv.Code = newCode;
                    Db.MobileReceiveItemDetails.Add(new MobileReceiveItemDetail
                    {
                        Code = rcv.Code,
                        LineNo = ++i,
                        ItemId = rcv.ItemId,
                        Qty = rcv.Qty,
                        TransDetailId = rcv.TransDetailId,
                        Type = rcv.Type,
                        UnitId = rcv.UnitId,
                        UomId = rcv.UomId,
                        WarehouseCode = rcv.WarehouseCode
                    });
                }

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data;
            result.Message = "Data penerimaan barang berhasil disimpan.";
            return result;
        }
        #endregion
    }
}
