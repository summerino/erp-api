using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales
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
                        x.Code.Contains(search) || x.SalesInitial.Contains(search) || x.CustCode.StartsWith(search) ||
                        x.CustName.Contains(search));
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
                       where dlv.TransCode == code && dlv.Mark != "V"
                       select new { dlv.Code, dlv.Date, dlv.Mark };

            return data.ToDynamicList();
        }

        public IEnumerable<VwSalesOrderHeader> GetInCompleteInvoiceData(string searchBy, string search, string invCode)
        {
            var data = Db.VwSalesOrderHeaders.Where(x => new[] { "PR", "CMP" }.Contains(x.Mark));

            if (!string.IsNullOrEmpty(search))
            {
                data = searchBy switch
                {
                    "code" => data.Where(x => x.Code.Contains(search)),
                    "date" => data.Where(x => x.Date == Convert.ToDateTime(search)),
                    "custName" => data.Where(x => x.CustName.Contains(search)),
                    _ => data
                };
            }

            data = string.IsNullOrWhiteSpace(invCode)
                ? data.Where(x => Db.SalesDeliveryHeaders
                    .Where(r => r.Mark == "A" && r.SrcTrans == 1)
                    .Select(r => r.TransCode).Contains(x.Code))
                : data.Where(x => Db.SalesDeliveryHeaders
                                      .Where(r => r.Mark == "A" && r.SrcTrans == 1)
                                      .Select(r => r.TransCode).Contains(x.Code) ||
                                  Db.SalesInvoiceHeaders
                                      .Where(i => i.Code == invCode)
                                      .Select(i => i.SoCode).Contains(x.Code));

            return searchBy switch
            {
                "code" => data.OrderBy(x => x.Code),
                "date" => data.OrderBy(x => x.Date),
                "custName" => data.OrderBy(x => x.CustName),
                _ => data
            };
        }

        public SaveResult Insert(SalesOrderRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking order qty is excess or not
                var checkQty = Db.SystemParameters.FirstOrDefault(x => x.Code == "DEF_SLS_ORD_CHECK_QTY")?.Value == "1";
                
                if (checkQty && IsQtyExcess(data.WarehouseCode, data.ItemDetails, null))
                {
                    result.Message = "Data order penjualan tidak bisa disimpan karena qty yang dipesan lebih besar dari qty yang tersedia.";
                    return result;
                }

                if (!CheckCreditLimit(data.CustCode, data.Total)) 
                {
                    result.Message = "Nilai transaksi lebih besar dari nilai batas kredit.";
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

                    if (data.IsSoDlv || data.IsSoInv || item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                    {
                        Db.SaveChanges();
                        listIdDetail.Add(orderDetail.Id);
                    }

                    if (item.DiscountItemDetails != null)
                    {
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
                        }
                        Db.SaveChanges();
                    }

                    if (item.FreeItemDetails != null)
                    {
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
                        }
                        Db.SaveChanges();
                    }
                }

                // Update Credit Used
                UpdateCreditUsed(data.CustCode, data.Total);

                if (data.IsSoDlv)
                {
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        SrcTrans = 1,
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
                            SoDetailId = listIdDetail[j - 1],
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
                        SrcTrans = 1,
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
                            SoDetailId = listIdDetail[j - 1],
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

                if (data.IsSoDlv || data.IsSoInv)
                {
                    var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        dlvData?.Code, data.Date, newCode);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);
                }

                //if (data.IsSoInv)
                //{
                //    var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);

                //    // Execute sp_update_stock_mutation_from_rcv
                //    Db.Database.ExecuteSqlRaw(
                //        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                //        dlvData?.Code, data.Date, newCode);

                //    // Execute sp_update_po_rcv_qty
                //    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);

                //    // Update sales order to closed if all sales delivery are invoiced
                //    if (
                //        !Db.SalesDeliveryHeaders
                //            .Any(x => x.TransCode == newCode && x.Mark != "INV"))
                //    {
                //        Db.Database.ExecuteSqlRaw(
                //            "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newCode);
                //    }
                //}
                
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
            var listIdDetail = new List<long>();


            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesOrderHeaders.Any(x => x.Code == data.Code && new[] { "V", "CLS" }.Contains(x.Mark)))
                {
                    result.Message = "Data order penjualan tidak bisa diubah karena sudah ditandai sebagai void atau closed.";
                    return result;
                }

                // Checking order qty is excess or not
                var checkQty = Db.SystemParameters.FirstOrDefault(x => x.Code == "DEF_SLS_ORD_CHECK_QTY")?.Value == "1";

                if (checkQty && IsQtyExcess(data.WarehouseCode, data.ItemDetails, data.Code))
                {
                    result.Message = "Data order penjualan tidak bisa disimpan karena qty yang dipesan lebih besar dari qty tersedia.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Restore Credit Used
                RestoreCreditUsed(data.Code, data.CustCode);

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
                    if (item.Id <= 0)
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

                        if (data.IsSoDlv || data.IsSoInv || item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            Db.SaveChanges();
                            listIdDetail.Add(orderDetail.Id);
                        }
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        if (data.IsSoDlv || data.IsSoInv || item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            listIdDetail.Add(item.Id);
                        }
                    }

                    if (item.DiscountItemDetails != null)
                    {
                        //Remove deleted detail
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
                                        OrderDetailId = listIdDetail[i - 1],
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
                    }

                    if (item.FreeItemDetails != null)
                    {
                        //Remove Deleted detail
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
                                        OrderDetailId = listIdDetail[i - 1],
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
                }

                // Update Credit Used
                UpdateCreditUsed(data.CustCode, data.Total);

                if (data.IsSoDlv)
                {
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        SrcTrans = 1,
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
                            SoDetailId = listIdDetail[j - 1],
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
                            SrcTrans = 1,
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
                                SoDetailId = listIdDetail[j - 1],
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

                if (data.IsSoDlv || data.IsSoInv)
                {
                    var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == data.Code);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        dlvData?.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);
                }

                //if (data.IsSoInv)
                //{
                //    var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == data.Code);

                //    // Execute sp_update_stock_mutation_from_rcv
                //    Db.Database.ExecuteSqlRaw(
                //        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                //        dlvData?.Code, data.Date, data.Code);

                //    // Execute sp_update_po_rcv_qty
                //    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);

                //    // Check all sales delivery are invoiced
                //    if (
                //        !Db.SalesDeliveryHeaders
                //            .Any(x => x.TransCode == data.Code && x.Mark != "INV"))
                //    {
                //        // Update sales order to closed
                //        Db.Database.ExecuteSqlRaw(
                //            "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.Code);
                //    }
                //    else
                //    {
                //        // Update sales order to partial receive or completed
                //        var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.Code && x.Qty > x.QtyDlv)
                //            ? "PS"
                //            : "CMP";

                //        Db.Database.ExecuteSqlRaw(
                //            "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.Code);
                //    }
                //}

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

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}, {2}",
                    data.Code, data.Date, true);

                // Restore Credit Used
                RestoreCreditUsed(code, data.CustCode);
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

                RestoreCreditUsedFromCloseAction(code, data.CustCode);
            }
            result.Success = true;
            result.Message = "Data order penjualan berhasil ditutup.";
            return result;
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
                    if(code == null)
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

        public IEnumerable<DetailFreeGoodData> GetFreeDetailData(string code, bool? fullDlv)
        {
            var result = (from dc in Db.SalesOrderDetailFreeGoods
                          join i in Db.Items on dc.ItemId equals i.Id
                          join u in Db.UoMConversions on dc.UnitId equals u.Id
                          where dc.Code == code
                          select new DetailFreeGoodData
                          {
                              Id = dc.Id,
                              Code = dc.Code,
                              OrderDetailId = dc.OrderDetailId,
                              LineNo = dc.LineNo,
                              PromoCode = dc.PromoCode,
                              ItemId = dc.ItemId,
                              UomId = dc.UomId,
                              UnitId = dc.UnitId,
                              Qty = dc.Qty,
                              QtyClosed = dc.QtyClosed,
                              UnitPrice = dc.UnitPrice,
                              CoaCode = dc.CoaCode,
                              Initial = i.Initial,
                              Name = i.Name,
                              UnitName = u.UnitEquivalent
                          }).ToList();

            if (fullDlv.HasValue)
            {
                result = (bool)fullDlv
                    ? result.Where(x => x.Qty <= x.QtyClosed).ToList()
                    : result.Where(x => x.Qty > x.QtyClosed).ToList();
            }
            return result;
        }

        public IEnumerable<SalesOrderDetailDiscount> GetDiscDetailData(string code)
        {
            return Db.SalesOrderDetailDiscounts.Where(x => x.Code == code).ToList();
        }

        #region Credit Used - Limit
        private bool CheckCreditLimit(string custCode, decimal total) => Db.Customers.Any(c => c.Code.Equals(custCode) && (c.PaymentTermId == 1 || (c.CreditLimit - c.CreditUsed) >= total)); 
        private void RestoreCreditUsed(string transCode, string custCode)
        {
            var prevAmount = Db.SalesOrderHeaders.AsNoTracking().FirstOrDefault(x => x.Code.Equals(transCode))?.Total;
            string query = $"update General.Customer set CreditUsed= (CreditUsed - {prevAmount}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        private void RestoreCreditUsedFromCloseAction(string transCode, string custCode)
        {
            var amount = (from x in Db.SalesInvoiceHeaders
                          where x.SoCode == transCode
                          select x.Total - x.PaidAmount).FirstOrDefault();
            string query = $"update General.Customer set CreditUsed= (CreditUsed - {amount}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        private void UpdateCreditUsed(string custCode, decimal total)
        {
            string query = $"update General.Customer set CreditUsed= (CreditUsed + {total}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        #endregion
    }
}
