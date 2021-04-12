using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model;
using ERP_API.Model.Purchase;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.Purchase
{
    public class PurchaseReceiveService : GeneralService<PurchaseReceiveHeader>, IPurchaseReceiveService
    {
        public PurchaseReceiveService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwPurchaseReceiveHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.PoCode == search ||
                        x.ReceiveInitial.Contains(search) || x.RefNo.StartsWith(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwPurchaseReceiveDetail> GetDetailData(string code)
        {
            return Db.VwPurchaseReceiveDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(PurchaseReceiveRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.PoCode))
                {
                    result.Message = "Can't update purchase receive because purchase order already mark as void or close.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.Code, data.PoCode, data.ItemDetails))
                {
                    result.Message = "Can't update purchase receive because receive qty bigger than outstanding qty.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("RCV_NUM_FMT", data.Date);
                    
                // Insert header data
                data.Code = newCode;
                Db.PurchaseReceiveHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        PoDetailId = item.PoDetailId,
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
                        Type = item.Type
                    });
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.PoCode);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success insert purchase receive.";
            return result;
        }

        public SaveResult Update(PurchaseReceiveRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.PurchaseReceiveHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Can't update purchase receive because data already mark as void.";
                    return result;
                }

                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.PoCode))
                {
                    result.Message = "Can't update purchase receive because purchase order already mark as void or close.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.Code, data.PoCode, data.ItemDetails))
                {
                    result.Message = "Can't update purchase receive because receive qty bigger than outstanding qty.";
                    return result;
                }

                // Update header data
                Db.PurchaseReceiveHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete detail data that doesn't have in data item details
                var delDetails = Db.PurchaseReceiveDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                foreach (var item in delDetails)
                {
                    Db.PurchaseReceiveDetails.Remove(item);
                }

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id == 0)
                    {
                        Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                        {
                            Code = item.Code,
                            LineNo = ++i,
                            PoDetailId = item.PoDetailId,
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
                            Type = item.Type
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PurchaseReceiveDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.PoCode);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update purchase receive.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PurchaseReceiveHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Can't void purchase receive because data already mark as void.";
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

                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.PoCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Success void purchase receive.";
            return result;
        }

        private bool IsPurchaseOrderInvalid(string poCode)
        {
            return Db.PurchaseOrderHeaders.Any(x => x.Code == poCode && new[] { "V", "CLS" }.Contains(x.Mark));
        }

        private bool IsQtyExcess(string code, string poCode, IEnumerable<PurchaseReceiveDetail> items)
        {
            // Get purchase receive lists
            var rcvCodeList = Db.PurchaseReceiveHeaders
                .Where(x => x.PoCode == poCode && x.Mark != "V" && x.Code != code)
                .Select(x => x.Code).ToList();

            // Calculate receive qty
            var rcvD = Db.PurchaseReceiveDetails
                .Where(x => rcvCodeList.Contains(x.Code) && x.Type == 0)
                .GroupBy(x => new { x.ItemId, x.UnitId })
                .Select(x => new
                {
                    x.Key.ItemId, x.Key.UnitId,
                    QtyRcv = x.Sum(r => (decimal?)r.Qty)
                });

            // Calculate outstanding qty
            var ordD = (
                from o in Db.PurchaseOrderDetails
                where o.Code == poCode && o.Type == 0
                join r in rcvD
                    on new { o.ItemId, o.UnitId } equals new { r.ItemId, r.UnitId } into rs
                from r in rs.DefaultIfEmpty()
                select new
                {
                    o.ItemId, o.UnitId, o.Qty,
                    Oustanding = o.Qty - (r.QtyRcv ?? 0m)
                }).ToList();

            // Checking receive qty from item details is excess or not
            var isExcess = (
                from o in ordD
                join d in items.Where(x => x.Type == 0)
                    on new { o.ItemId, o.UnitId } equals new { d.ItemId, d.UnitId } into ds
                from d in ds.DefaultIfEmpty()
                where o.Oustanding < d.Qty
                select new
                {
                    o.ItemId, o.UnitId, o.Oustanding
                }).Any();

            return isExcess;
        }

    }
}
