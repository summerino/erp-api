using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;
using ERP_API.Domain.Entities.Inventory;

namespace ERP_API.Domain.Services.Purchase
{
    public class PurchaseReturnService : GeneralService<PurchaseReturnHeader>, IPurchaseReturnService
    {
        public PurchaseReturnService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwPurchaseReturnHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.RcvCode == search ||
                        x.ShippedInitial.Contains(search) || x.RefNo.StartsWith(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwPurchaseReturnDetail> GetDetailData(string code, bool? fullReceived)
        {
            var data = Db.VwPurchaseReturnDetails.Where(x => x.Code == code);

            if (fullReceived.HasValue)
            {
                data = (bool)fullReceived
                    ? data.Where(x => x.Qty <= x.QtyRcv)
                    : data.Where(x => x.Qty > x.QtyRcv);
            }

            return data.OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var piD = from dt in Db.PurchaseInvoiceDetails
                      where dt.RcvCode == code
                      select dt.Code;

            var data = from piH in Db.PurchaseInvoiceHeaders
                       where piD.Contains(piH.Code) && piH.Mark == "A"
                       select new { piH.Code, piH.Date, piH.Total };

            return data.ToDynamicList();
        }

        public IEnumerable<PurchaseReceiveHeader> GetUnInvoiceData(string poCode, string invCode)
        {
            var data = Db.PurchaseReceiveHeaders.Where(x => x.TransCode == poCode);

            data = string.IsNullOrWhiteSpace(invCode)
                ? data.Where(x => x.Mark == "A")
                : data.Where(x => x.Mark == "A" ||
                                  Db.PurchaseInvoiceDetails
                                      .Where(i => i.Code == invCode)
                                      .Select(i => i.RcvCode).Contains(x.Code));

            return data;
        }

        public SaveResult Insert(PurchaseReturnRequest data)
        {
            var result = new SaveResult(false);
            var dbtMemo = new DebitMemo();
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking purchase receive mark
                if (IsPurchaseReceiveInvalid(data.RcvCode))
                {
                    result.Message = "Can't insert purchase return because purchase receive already mark as void or close.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.ItemDetails))
                {
                    result.Message = "Can't insert purchase return because return qty bigger than outstanding qty.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("PR_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.PurchaseReturnHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.PurchaseReturnDetails.Add(new PurchaseReturnDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        RcvDetailId = item.RcvDetailId,
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
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp,
                        WarehouseCode = item.WarehouseCode,
                        QtyRcv = item.QtyRcv,
                        WarehouseCodeIn = item.WarehouseCodeIn
                    });
                }

                var dbtCode = GetNewCode("DM_NUM_FMT", data.Date);

                dbtMemo = new DebitMemo {
                    Code = dbtCode,
                    Date = data.Date,
                    SrcTrans = (short)(data.Type == 1 ? 2 : 3),
                    SupCode = data.SupCode,
                    TransCode = data.Code,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    Amount = data.Total,
                    Used = 0,
                    Notes = data.RcvCode != null ? "Automatically created by Purchase Return " + newCode : "Automatically created by Purchase Return W/O Doc. " + newCode,
                    Mark = "A",
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.DebitMemos.Add(dbtMemo);

                Db.SaveChanges();

                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rtn {0}, {1}, {2}",
                    data.Code, data.Date, data.RcvCode);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = dbtMemo;
            result.Message = "Success insert purchase return.";
            return result;
        }

        public SaveResult Update(PurchaseReturnRequest data)
        {
            var result = new SaveResult(false);
            var dbtMemo = new DebitMemo();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.PurchaseReturnHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Can't update purchase return because data already mark as void.";
                    return result;
                }

                // Checking purchase order mark
                if (IsPurchaseReceiveInvalid(data.RcvCode))
                {
                    result.Message = "Can't update purchase return because purchase receive already mark as void or close.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.ItemDetails))
                {
                    result.Message = "Can't update purchase return because return qty bigger than outstanding qty.";
                    return result;
                }

                // Update header data
                Db.PurchaseReturnHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in return before
                var delDetails = Db.PurchaseReturnDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in return before
                Db.PurchaseReturnDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.PurchaseReturnDetails.Add(new PurchaseReturnDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            RcvDetailId = item.RcvDetailId,
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
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp,
                            WarehouseCode = item.WarehouseCode,
                            QtyRcv = item.QtyRcv,
                            WarehouseCodeIn = item.WarehouseCodeIn
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PurchaseReturnDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                dbtMemo = Db.DebitMemos.FirstOrDefault(x => x.TransCode == data.Code);
                dbtMemo.SrcTrans = (short)(data.Type == 1 ? 2 : 3);
                dbtMemo.SupCode = data.SupCode;
                dbtMemo.CurrCode = data.CurrCode;
                dbtMemo.Rate = data.Rate;
                dbtMemo.Amount = data.Total;
                dbtMemo.UpdatedBy = data.UpdatedBy;
                dbtMemo.UpdatedDate = data.UpdatedDate;
                Db.DebitMemos.Update(dbtMemo);
                Db.Entry(dbtMemo).Property(e => e.Code).IsModified = false;
                Db.Entry(dbtMemo).Property(e => e.TransCode).IsModified = false;
                Db.Entry(dbtMemo).Property(e => e.Notes).IsModified = false;
                Db.Entry(dbtMemo).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(dbtMemo).Property(e => e.CreatedDate).IsModified = false;


                // Save changes
                Db.SaveChanges();

                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rtn {0}, {1}, {2}",
                    data.Code, data.Date, data.RcvCode);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = dbtMemo;
            result.Message = "Success update purchase return.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PurchaseReturnHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Can't void purchase return because data already mark as void.";
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
                    if(stockPR != null)
                    {
                        Db.StockMutations.RemoveRange(stockPR);
                    }

                    // Save changes
                    Db.SaveChanges();

                    Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rtn {0}, {1}, {2}",
                    data.Code, data.Date, data.RcvCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Success void purchase return.";
            return result;
        }

        private bool IsPurchaseReceiveInvalid(string rcvCode)
        {
            return Db.PurchaseReceiveHeaders.Any(x => x.Code == rcvCode && new[] { "V", "CLS" }.Contains(x.Mark));
        }

        private bool IsQtyExcess(IEnumerable<PurchaseReturnDetail> items)
        {
            var result = false;
            foreach (var item in items)
            {
                var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
                var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == item.WarehouseCode && x.ItemId == item.ItemId);
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
            return result;
        }
    }
}
