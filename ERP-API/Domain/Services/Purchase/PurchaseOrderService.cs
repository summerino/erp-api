using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Services.Purchase
{
    public class PurchaseOrderService : GeneralService<PurchaseOrderHeader>, IPurchaseOrderService
    {
        public PurchaseOrderService(TenantContext db)
            : base(db)
        {
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

        public SaveResult Insert(PurchaseOrderRequest data)
        {
            var result = new SaveResult(false);

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
                    Db.PurchaseOrderDetails.Add(new PurchaseOrderDetail
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
            result.Data = data.Code;
            result.Message = "Success insert purchase order.";
            return result;
        }

        public SaveResult Update(PurchaseOrderRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.PurchaseOrderHeaders.Any(x => x.Code == data.Code & x.Mark == "V"))
                {
                    result.Message = "Can't update purchase order because data already mark as void.";
                    return result;
                }

                // Update header data
                Db.PurchaseOrderHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete detail data that doesn't have in data item details
                var delDetails = Db.PurchaseOrderDetails
                    .Where(d => d.Code == data.Code & !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                foreach (var item in delDetails)
                {
                    Db.PurchaseOrderDetails.Remove(item);
                }

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id == 0)
                    {
                        Db.PurchaseOrderDetails.Add(new PurchaseOrderDetail
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
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PurchaseOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
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
            result.Data = data.Code;
            result.Message = "Success update purchase order.";
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
                    result.Message = "Can't void purchase order because data already mark as void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success void purchase order.";
            return result;
        }
    }
}
