using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domain.Services.Purchase
{
    public class PurchaseReceiveService : GeneralService<PurchaseReceiveHeader>, IPurchaseReceiveService
    {
        public PurchaseReceiveService(TenantContext db)
            : base(db)
        {
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
                        ItemId = item.ItemId,
                        OrderQty = item.OrderQty,
                        OutstandingQty = item.OutstandingQty,
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

                // Execute sp_update_po_mark
                Db.Database.ExecuteSqlRaw("EXEC sp_update_po_mark {0}", data.PoCode);

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
                if (Db.PurchaseReceiveHeaders.Any(x => x.Code == data.Code & x.Mark == "V"))
                {
                    result.Message = "Can't update purchase receive because data already mark as void.";
                    return result;
                }

                // Update header data
                Db.PurchaseReceiveHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete detail data that doesn't have in data item details
                var delDetails = Db.PurchaseReceiveDetails
                    .Where(d => d.Code == data.Code & !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
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
                            ItemId = item.ItemId,
                            OrderQty = item.OrderQty,
                            OutstandingQty = item.OutstandingQty,
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

                // Execute sp_update_po_mark
                Db.Database.ExecuteSqlRaw("EXEC sp_update_po_mark {0}", data.PoCode);

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

                    // Execute sp_update_po_mark
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_mark {0}", data.PoCode);

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
    }
}
