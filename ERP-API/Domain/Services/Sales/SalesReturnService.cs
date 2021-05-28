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
    public class SalesReturnService : GeneralService<SalesReturnHeader>, ISalesReturnService
    {
        public SalesReturnService(TenantContext db)
            :base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwSalesReturnHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustName.Contains(search) || x.TransCode == search ||
                        x.SalesInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesReturnDetail> GetDetailData(string code, bool? fullDelivered)
        {
            var data = Db.VwSalesReturnDetails.Where(x => x.Code == code);

            if (fullDelivered.HasValue)
            {
                data = (bool)fullDelivered
                    ? data.Where(x => x.Qty <= x.QtyDlv)
                    : data.Where(x => x.Qty > x.QtyDlv);
            }

            return data.OrderBy(x => x.LineNo);
        }

        public IEnumerable<VwSalesReturnDetailExchDiffItem> GetDetailExchangeData(string code)
        {
            var data = Db.VwSalesReturnDetailExchDiffItems.Where(x => x.Code == code);
            return data.OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var data = from dt in Db.VwCreditMemos
                       where dt.TransCode == code
                       select dt;

            return data.ToDynamicList();
        }

        public SaveResult Insert(SalesReturnRequest data)
        {
            var result = new SaveResult(false);
            var cdtMemo = new CreditMemo();
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking receive qty is excess or not
                if (IsQtyExcess(data.WarehouseCode,data.ItemDetails))
                {
                    result.Message = "Data pengembalian penjualan tidak bisa disimpan karena qty yg dikembalikan lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("SR_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.SalesReturnHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesReturnDetails.Add(new SalesReturnDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        TransDetailId = item.TransDetailId,
                        ItemId = item.ItemId,
                        Qty = item.Qty,
                        QtyDlv = 0,
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
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                

                if(data.Type == 1)
                {
                    var cdtCode = GetNewCode("CM_NUM_FMT", data.Date);

                    cdtMemo = new CreditMemo
                    {
                        Code = cdtCode,
                        Date = data.Date,
                        SrcTrans = (short)(data.Type == 1 ? 2 : 3),
                        CustCode = data.CustCode,
                        TransCode = data.Code,
                        CurrCode = data.CurrCode,
                        Rate = data.Rate,
                        Amount = data.Total,
                        Used = 0,
                        //Notes = data.RcvCode != null ? "Automatically created by Sales Return " + newCode : "Automatically created by Sales Return W/O Doc. " + newCode,
                        Mark = "A",
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.CreditMemos.Add(cdtMemo);
                }
                
                Db.SaveChanges();

                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_sr {0}, {1}, {2}, {3}",
                    data.Code, data.Date, data.TransCode, data.WarehouseCode);

                if (data.Type == 3)
                {
                    var exchangeDetail = data.DiffItemDetails;

                    foreach (var item in exchangeDetail)
                    {
                        Db.SalesReturnDetailExchDiffItems.Add(new SalesReturnDetailExchDiffItem
                        {
                            Code = newCode,
                            LineNo = ++i,
                            //ReturnDetailId = item.Id,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            QtyDlv = item.QtyDlv,
                            WarehouseCode = item.WarehouseCode,
                            UnitPrice = item.UnitPrice,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }

                    Db.SaveChanges();
                }
                

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = cdtMemo;
            result.Message = "Data pengembalian penjualan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesReturnRequest data)
        {
            var result = new SaveResult(false);
            var cdtMemo = new CreditMemo();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesReturnHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data pengembalian penjualan tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.WarehouseCode ,data.ItemDetails))
                {
                    result.Message = "Data pengembalian penjualan tidak bisa diubah karena qty yg dikembalikan lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Update header data
                Db.SalesReturnHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in return before
                var delDetails = Db.SalesReturnDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in return before
                Db.SalesReturnDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.SalesReturnDetails.Add(new SalesReturnDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            TransDetailId = item.TransDetailId,
                            ItemId = item.ItemId,
                            Qty = item.Qty,
                            QtyDlv = item.QtyDlv,
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
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesReturnDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                if (data.Type == 1)
                {
                    cdtMemo = Db.CreditMemos.FirstOrDefault(x => x.TransCode == data.Code);
                    cdtMemo.SrcTrans = (short)(data.Type == 1 ? 2 : 3);
                    cdtMemo.CustCode = data.CustCode;
                    cdtMemo.CurrCode = data.CurrCode;
                    cdtMemo.Rate = data.Rate;
                    cdtMemo.Amount = data.Total;
                    cdtMemo.UpdatedBy = data.UpdatedBy;
                    cdtMemo.UpdatedDate = data.UpdatedDate;
                    Db.CreditMemos.Update(cdtMemo);
                    Db.Entry(cdtMemo).Property(e => e.Code).IsModified = false;
                    Db.Entry(cdtMemo).Property(e => e.TransCode).IsModified = false;
                    Db.Entry(cdtMemo).Property(e => e.Notes).IsModified = false;
                    Db.Entry(cdtMemo).Property(e => e.CreatedBy).IsModified = false;
                    Db.Entry(cdtMemo).Property(e => e.CreatedDate).IsModified = false;
                }


                // Save changes
                Db.SaveChanges();

                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_sr {0}, {1}, {2}, {3}",
                    data.Code, data.Date, data.TransCode, data.WarehouseCode);

                if (data.Type == 3)
                {
                    var delExchange = Db.SalesReturnDetailExchDiffItems.Where(d => d.Code == data.Code && !data.DiffItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                    Db.SalesReturnDetailExchDiffItems.RemoveRange(delExchange);

                    var exchangeDetail = data.DiffItemDetails;

                    foreach (var item in exchangeDetail)
                    {
                        Db.SalesReturnDetailExchDiffItems.Add(new SalesReturnDetailExchDiffItem
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            //ReturnDetailId = (long)item.TransDetailId,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            QtyDlv = item.QtyDlv,
                            WarehouseCode = item.WarehouseCode,
                            UnitPrice = item.UnitPrice,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }

                    Db.SaveChanges();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = cdtMemo;
            result.Message = "Data pengembalian penjualan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesReturnHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data pengembalian penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                using var transaction = Db.Database.BeginTransaction();
                try
                {
                    // Update header data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    var stockPR = Db.StockMutations.Where(x => x.RefCode1 == data.Code);
                    if (stockPR != null)
                    {
                        Db.StockMutations.RemoveRange(stockPR);
                    }

                    // Save changes
                    Db.SaveChanges();

                    Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_sr {0}, {1}, {2}, {3}",
                    data.Code, data.Date, data.TransCode, data.WarehouseCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data pengembalian penjualan berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsQtyExcess(string warehouseCode,IEnumerable<SalesReturnDetail> items)
        {
            var result = false;
            foreach (var item in items)
            {
                var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
                var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == item.ItemId);
                var stockM = Db.StockMutations.Where(x => x.WarehouseCode == warehouseCode && x.UomId == item.UomId).Sum(x => x.BaseQty);
                if (stock != null)
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > stock.QtyOnHand)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > stock.QtyOnHand)
                        {
                            result = true;
                        }
                    }
                }
                else if (!stockM.Equals(null)) // check to stock mutation if warehouse quantites not available
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > stockM)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > stockM)
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
