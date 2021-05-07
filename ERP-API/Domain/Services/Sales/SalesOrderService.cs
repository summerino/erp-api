using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;
using Microsoft.EntityFrameworkCore;

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
                       where dlv.SoCode == code && dlv.Mark == "A"
                       select new { dlv.Code, dlv.Date, dlv.Mark };

            return data.ToDynamicList();
        }

        public SaveResult Insert(SalesOrderRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
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
                        SoCode = newCode,
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

                Db.SaveChanges();

                if (data.IsSoDlv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.SoCode == newCode);
                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        DlvData.Code, data.Date, newCode);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);
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
            result.Message = "Success insert sales order.";
            return result;
        }

        public SaveResult Update(SalesOrderRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesOrderHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Can't update sales order because data already mark as void.";
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
                        SoCode = data.Code,
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

                Db.SaveChanges();

                if (data.IsSoDlv)
                {
                    var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.SoCode == data.Code);
                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                        DlvData.Code, data.Date, data.Code);

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);
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
            result.Message = "Success update sales order.";
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
                    result.Message = "Can't void sales order because data already mark as void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success void sales order.";
            return result;
        }
    }
}
