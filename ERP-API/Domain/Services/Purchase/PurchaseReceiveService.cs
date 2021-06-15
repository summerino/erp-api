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
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.TransCode == search ||
                        x.ReceiveInitial.Contains(search) || x.RefNo.StartsWith(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwPurchaseReceiveDetail> GetDetailData(string code)
        {
            return Db.VwPurchaseReceiveDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
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

        public SaveResult Insert(PurchaseReceiveRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.TransCode))
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data order pembelian sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.SrcTrans, data.TransCode, data.ItemDetails, null))
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena qty yg diterima lebih besar dari qty yang tersedia.";
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
                        TransDetailId = item.TransDetailId,
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
                
                if(data.SrcTrans == 2)
                {
                    var dataPR = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);
                    if (dataPR.Type == 1) {
                        // Update Debit Memo
                        var dbtMemo = Db.DebitMemos.FirstOrDefault(x => x.TransCode == data.TransCode);
                        var availableAmount = dbtMemo.Amount - dbtMemo.Used;
                        if(data.Total > availableAmount)
                        {
                            result.Message = "Tidak bisa disimpan karena jumlah total penerimaan pembeliam lebih besar dari jumlah total memo debit.";
                            return result;
                        }
                        availableAmount -= data.Total;
                        dbtMemo.Used += data.Total;
                        dbtMemo.Mark = availableAmount == 0 ? "FU" : "PU";
                        dbtMemo.UpdatedBy = data.UpdatedBy;
                        dbtMemo.UpdatedDate = data.UpdatedDate;
                        Db.DebitMemos.Update(dbtMemo);

                        // Update Purchase Return Detail
                        var prData = Db.PurchaseReturnDetails.Where(x => x.Code == data.TransCode).ToList();
                        foreach (var item in data.ItemDetails)
                        {
                            var itemPr = prData.FirstOrDefault(x => x.ItemId == item.ItemId);
                            var availableQty = itemPr.Qty - itemPr.QtyRcv;
                            if (item.Qty > availableQty)
                            {
                                result.Message = "Data penerimaan pembelian tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia";
                                return result;
                            }

                            itemPr.QtyRcv += item.Qty;
                            Db.PurchaseReturnDetails.Update(itemPr);
                        }

                        // Update Purchase Return Header
                        var prhData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);
                        prhData.Mark = dbtMemo.Mark == "A" ? "A" : (dbtMemo.Mark == "PU" ? "PR" : "CMP");
                        Db.PurchaseReturnHeaders.Update(prhData);
                    }
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                    data.Code, data.Date, data.TransCode);

                if (data.SrcTrans == 1)
                {
                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.TransCode);
                }
                else
                {
                    // Execute sp_update_pr_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_pr_rcv_qty {0}", data.TransCode);
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
            result.Message = "Data penerimaan pembelian berhasil disimpan.";
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
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.TransCode))
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data order pembelian sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking receive qty is excess or not
                if (IsQtyExcess(data.SrcTrans, data.TransCode, data.ItemDetails, data.Code))
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Update header data
                Db.PurchaseReceiveHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in receive before
                var delDetails = Db.PurchaseReceiveDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in receive before
                Db.PurchaseReceiveDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            TransDetailId = item.TransDetailId,
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
                        Db.Entry(item).Property(e => e.TransDetailId).IsModified = false;
                    }
                }

                if (data.SrcTrans == 2)
                {
                    var dataPR = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);
                    if (dataPR.Type == 1)
                    {
                        // Update Debit Memo
                        var dbtMemo = Db.DebitMemos.FirstOrDefault(x => x.TransCode == data.TransCode);

                        var oldTotal = Db.PurchaseReceiveHeaders.Where(x => x.TransCode == data.TransCode && x.Mark == "A")
                            .GroupBy(x => new { x.TransCode })
                            .Select(g => new { g.Key.TransCode, SumTotal = g.Sum(x => x.Total) });

                        decimal valueTotal = 0;
                        if (oldTotal.FirstOrDefault().SumTotal > data.Total)
                        {
                            valueTotal = oldTotal.FirstOrDefault().SumTotal - data.Total;
                            dbtMemo.Used -= valueTotal;
                        }
                        else
                        {
                            valueTotal = data.Total - oldTotal.FirstOrDefault().SumTotal;
                            dbtMemo.Used += valueTotal;
                        }

                        if (dbtMemo.Used > dbtMemo.Amount)
                        {
                            result.Message = "Tidak bisa disimpan karena jumlah total penerimaan pembeliam lebih besar dari jumlah total memo debit.";
                            return result;
                        }

                        dbtMemo.Mark = dbtMemo.Used == 0 ? "A" : ((dbtMemo.Amount - dbtMemo.Used) == 0 ? "FU" : "PU");
                        dbtMemo.UpdatedBy = data.UpdatedBy;
                        dbtMemo.UpdatedDate = data.UpdatedDate;
                        Db.DebitMemos.Update(dbtMemo);

                        // Update Purchase Return Detail
                        var prData = Db.PurchaseReturnDetails.Where(x => x.Code == data.TransCode).ToList();
                        foreach (var item in data.ItemDetails)
                        {
                            var itemPrcv = Db.PurchaseReceiveDetails.FirstOrDefault(x => x.Id == item.Id);
                            var itemPr = prData.FirstOrDefault(x => x.ItemId == item.ItemId);

                            decimal valueQty = 0;
                            if (itemPr.QtyRcv > item.Qty)
                            {
                                valueQty = itemPr.QtyRcv - item.Qty;
                                itemPr.QtyRcv -= valueQty;
                            }
                            else
                            {
                                valueQty = item.Qty - itemPr.QtyRcv;
                                itemPr.QtyRcv += valueQty;
                            }

                            if (itemPr.QtyRcv > itemPr.Qty)
                            {
                                result.Message = "Data penerimaan pembelian tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia";
                                return result;
                            }

                            Db.PurchaseReturnDetails.Update(itemPr);
                        }

                        // Update Purchase Return Header
                        var prhData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);
                        prhData.Mark = dbtMemo.Mark == "A" ? "A" : (dbtMemo.Mark == "PU" ? "PR" : "CMP");
                        Db.PurchaseReturnHeaders.Update(prhData);
                    }
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                    data.Code, data.Date, data.TransCode);

                if (data.SrcTrans == 1)
                {
                    // Execute sp_update_po_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.TransCode);
                }
                else
                {
                    // Execute sp_update_pr_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_pr_rcv_qty {0}", data.TransCode);
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
            result.Message = "Data penerimaan pembelian berhasil diperbarui.";
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
                    result.Message = "Data penerimaan pembelian tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                using var transaction = Db.Database.BeginTransaction();
                try
                {
                    // Update header data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    if (data.SrcTrans == 2)
                    {
                        // Update Debit Memo
                        var dbtMemo = Db.DebitMemos.FirstOrDefault(x => x.TransCode == data.TransCode);

                        dbtMemo.Used -= data.Total;
                        dbtMemo.Mark = dbtMemo.Used == 0 ? "A" : ((dbtMemo.Amount - dbtMemo.Used) == 0 ? "FU" : "PU");
                        dbtMemo.UpdatedBy = data.UpdatedBy;
                        dbtMemo.UpdatedDate = data.UpdatedDate;
                        Db.DebitMemos.Update(dbtMemo);

                        // Update Purchase Return Detail
                        var prData = Db.PurchaseReturnDetails.Where(x => x.Code == data.TransCode).ToList();
                        var prcvData = Db.PurchaseReceiveDetails.Where(x => x.Code == data.Code).ToList();
                        foreach (var item in prcvData)
                        {
                            var itemPr = prData.FirstOrDefault(x => x.ItemId == item.ItemId);
                            itemPr.QtyRcv -= item.Qty;
                            Db.PurchaseReturnDetails.Update(itemPr);
                        }

                        // Update Purchase Return Header
                        var prhData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);
                        prhData.Mark = dbtMemo.Mark == "A" ? "A" : (dbtMemo.Mark == "PU" ? "PR" : "CMP");
                        Db.PurchaseReturnHeaders.Update(prhData);
                    }

                    // Save changes
                    Db.SaveChanges();

                    // Execute sp_update_po_rcv_qty
                    if (data.SrcTrans == 1)
                    {
                        // Execute sp_update_po_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", data.TransCode);
                    }
                    else
                    {
                        // Execute sp_update_pr_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_pr_rcv_qty {0}", data.TransCode);
                    }

                    // update stock in warehouse
                    var detailData = Db.PurchaseReceiveDetails.Where(x => x.Code == data.Code).ToList();
                    foreach (var item in detailData)
                    {
                        var stockMRcv = Db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == item.Id);
                        var stockMPo = Db.StockMutations.FirstOrDefault(x => x.RefCode1 == stockMRcv.RefCode2);
                        var wqItemRcv = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == stockMRcv.WarehouseCode && x.ItemId == stockMRcv.ItemId);
                        var wqItemPo = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == stockMPo.WarehouseCode && x.ItemId == stockMPo.ItemId);

                        wqItemRcv.QtyOnHand -= stockMRcv.BaseQty;
                        wqItemPo.QtyOnIndent += stockMRcv.BaseQty;

                        Db.WarehouseQuantities.Update(wqItemRcv);
                        Db.WarehouseQuantities.Update(wqItemPo);

                        Db.StockMutations.Remove(stockMRcv);
                        Db.SaveChanges();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data penerimaan pembelian berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsPurchaseOrderInvalid(string poCode)
        {
            return Db.PurchaseOrderHeaders.Any(x => x.Code == poCode && new[] { "V", "CLS" }.Contains(x.Mark));
        }

        private bool IsQtyExcess(int srcTrans, string transCode, IEnumerable<PurchaseReceiveDetail> items, string code)
        {
            var result = false;
            if (srcTrans == 1) // Purchase Order
            {
                foreach (var item in items)
                {
                    var dataPODetail = Db.PurchaseOrderDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId);
                    if (code == null)
                    {
                        var availableStock = dataPODetail.Qty - dataPODetail.QtyRcv;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var oldPRD = Db.PurchaseReceiveDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId);
                        var availableStock = dataPODetail.Qty - (dataPODetail.QtyRcv - oldPRD.Qty) ;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                }
            }
            else // Purchase Return
            {
                foreach (var item in items)
                {
                    var dataPRDetail = Db.PurchaseReturnDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId);
                    if (code == null)
                    {
                        var availableStock = dataPRDetail.Qty - dataPRDetail.QtyRcv ;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var oldPRD = Db.PurchaseReceiveDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId);
                        var availableStock = dataPRDetail.Qty - (dataPRDetail.QtyRcv - oldPRD.Qty);
                        if (item.Qty > availableStock)
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
