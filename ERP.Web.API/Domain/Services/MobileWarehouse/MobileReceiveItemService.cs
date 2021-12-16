using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileWarehouse
{
    public class MobileReceiveItemService : GeneralService<MobileReceiveItemHeader>, IMobileReceiveItemService
    {
        public MobileReceiveItemService(TenantContext db)
            :base(db)
        {
                
        }
        public SaveResult Approve(List<MobileReceiveItemHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                foreach (var itemData in data)
                {
                    var rcvDetailData = Db.MobileReceiveItemDetails.Where(x => x.Code == itemData.Code).ToList();
                    PurchaseReturnHeader rtnHeadData = new();
                    List<PurchaseReturnDetail> rtnDetailData = new();
                    PurchaseOrderHeader poHeadData = new();
                    List<PurchaseOrderDetail> poDetailData = new();


                    if (itemData.SrcTrans == 2)
                    {
                        rtnHeadData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == itemData.TransCode);
                        rtnDetailData = Db.PurchaseReturnDetails.Where(x => x.Code == itemData.TransCode).ToList();
                    }
                    else
                    {
                        poHeadData = Db.PurchaseOrderHeaders.FirstOrDefault(x => x.Code == itemData.TransCode);
                        poDetailData = Db.PurchaseOrderDetails.Where(x => x.Code == itemData.TransCode).ToList();
                    }

                    // Get new code
                    var newCode = GetNewCode("RCV_NUM_FMT", itemData.Date);

                    var rcvHeadData = new PurchaseReceiveHeader
                    {
                        Code = newCode,
                        Date = itemData.Date,
                        TransCode =  itemData.TransCode,
                        RefNo = itemData.Code,
                        SrcTrans = itemData.SrcTrans,
                        SupCode = itemData.SupCode,
                        ReceiveBy = itemData.ReceiveBy,
                        CurrCode = "IDR",
                        Rate = 1,
                        ShipmentFee = 0m,
                        HandlingFee = 0m,
                        SubTotal = 0m,
                        FinalDiscPercent = 0m,
                        FinalDisc = 0m,
                        IncludeTax = false,
                        TaxAmount = 0m,
                        Total = 0m,
                        Dpp = 0m,
                        PaidAmount = 0m,
                        Mark = "A",
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userId,
                        UpdatedDate = DateTime.Now,
                        ApprovedBy = userId,
                        ApprovedDate = DateTime.Now,
                    };

                    Db.PurchaseReceiveHeaders.Add(rcvHeadData);

                    // Insert detail data
                    short i = 0;
                    foreach (var itemDetail in rcvDetailData)
                    {
                        var itemMaster = Db.Items.FirstOrDefault(x => x.Id == itemDetail.ItemId);
                        if (rcvHeadData.SrcTrans == 2)
                        {
                            var detailData = rtnDetailData.FirstOrDefault(x => x.ItemId == itemDetail.ItemId);
                            var taxData = Db.Taxes.FirstOrDefault(x => x.Id == detailData.TaxId);
                            var taxAmount = rcvHeadData.IncludeTax ? Math.Round((detailData.UnitPrice - detailData.Disc) - ((detailData.UnitPrice - detailData.Disc) / (1 + (taxData.Rate / 100)))) 
                                : Math.Round((detailData.UnitPrice - detailData.Disc) * (taxData.Rate / 100));
                            var nettPrice = rcvHeadData.IncludeTax ? detailData.UnitPrice - detailData.Disc : detailData.UnitPrice - detailData.Disc + taxAmount;
                            var dpp = rcvHeadData.IncludeTax ? detailData.UnitPrice - detailData.Disc - taxAmount : detailData.UnitPrice - detailData.Disc;

                            Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                            {
                                Code = newCode,
                                LineNo = ++i,
                                TransDetailId = itemDetail.TransDetailId,
                                ItemId = itemDetail.ItemId,
                                Qty = itemDetail.Qty,
                                UomId = itemDetail.UomId,
                                UnitId = itemDetail.UnitId,
                                Length = itemMaster.Length,
                                Width = itemMaster.Width,
                                Height = itemMaster.Height,
                                Weight = itemMaster.Weight,
                                DimensionMeasurement = itemMaster.DimensionMeasurement,
                                WeightMeasurement = itemMaster.WeightMeasurement,
                                UnitPrice = detailData.UnitPrice,
                                Disc = detailData.Disc,
                                TaxId = detailData.TaxId,
                                TaxAmount = taxAmount,
                                NettPrice = nettPrice,
                                Total = itemDetail.Qty * nettPrice,
                                Dpp = dpp,
                                WarehouseCode = itemDetail.WarehouseCode,
                                Type = itemDetail.Type
                            });
                        } 
                        else
                        {
                            if (itemDetail.Type == 0)
                            {
                                var detailData = poDetailData.FirstOrDefault(x => x.ItemId == itemDetail.ItemId);
                                var taxData = Db.Taxes.FirstOrDefault(x => x.Id == detailData.TaxId);
                                var taxAmount = rcvHeadData.IncludeTax ? Math.Round((detailData.UnitPrice - detailData.Disc) - ((detailData.UnitPrice - detailData.Disc) / (1 + (taxData.Rate / 100))))
                                    : Math.Round((detailData.UnitPrice - detailData.Disc) * (taxData.Rate / 100));
                                var nettPrice = rcvHeadData.IncludeTax ? detailData.UnitPrice - detailData.Disc : detailData.UnitPrice - detailData.Disc + taxAmount;
                                var dpp = rcvHeadData.IncludeTax ? detailData.UnitPrice - detailData.Disc - taxAmount : detailData.UnitPrice - detailData.Disc;

                                Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                                {
                                    Code = newCode,
                                    LineNo = ++i,
                                    TransDetailId = itemDetail.TransDetailId,
                                    ItemId = itemDetail.ItemId,
                                    Qty = itemDetail.Qty,
                                    UomId = itemDetail.UomId,
                                    UnitId = itemDetail.UnitId,
                                    Length = itemMaster.Length,
                                    Width = itemMaster.Width,
                                    Height = itemMaster.Height,
                                    Weight = itemMaster.Weight,
                                    DimensionMeasurement = itemMaster.DimensionMeasurement,
                                    WeightMeasurement = itemMaster.WeightMeasurement,
                                    UnitPrice = detailData.UnitPrice,
                                    Disc = detailData.Disc,
                                    TaxId = detailData.TaxId,
                                    TaxAmount = taxAmount,
                                    NettPrice = nettPrice,
                                    Total = itemDetail.Qty * nettPrice,
                                    Dpp = dpp,
                                    WarehouseCode = itemDetail.WarehouseCode,
                                    Type = itemDetail.Type
                                });
                            }
                            else
                            {
                                var mItem = Db.Items.FirstOrDefault(x => x.Id == itemDetail.ItemId);
                                var taxData = Db.Taxes.FirstOrDefault(x => x.Id == mItem.PurchaseTaxId);
                                var taxAmount = mItem.BuyPrice.Value > 0 ? (rcvHeadData.IncludeTax ? Math.Round(mItem.BuyPrice.Value - (mItem.BuyPrice.Value / (1 + (taxData.Rate / 100))))
                                    : Math.Round(mItem.BuyPrice.Value * (taxData.Rate / 100))) : 0m;
                                var nettPrice = mItem.BuyPrice.Value > 0 ? (rcvHeadData.IncludeTax ? mItem.BuyPrice.Value : mItem.BuyPrice.Value + taxAmount) : 0m;
                                var dpp = mItem.BuyPrice.Value > 0 ?  (rcvHeadData.IncludeTax ? mItem.BuyPrice.Value - taxAmount : mItem.BuyPrice.Value) : 0m;

                                Db.PurchaseReceiveDetails.Add(new PurchaseReceiveDetail
                                {
                                    Code = newCode,
                                    LineNo = ++i,
                                    TransDetailId = itemDetail.TransDetailId,
                                    ItemId = itemDetail.ItemId,
                                    Qty = itemDetail.Qty,
                                    UomId = itemDetail.UomId,
                                    UnitId = itemDetail.UnitId,
                                    Length = itemMaster.Length,
                                    Width = itemMaster.Width,
                                    Height = itemMaster.Height,
                                    Weight = itemMaster.Weight,
                                    DimensionMeasurement = itemMaster.DimensionMeasurement,
                                    WeightMeasurement = itemMaster.WeightMeasurement,
                                    UnitPrice = mItem?.BuyPrice.Value ?? 0m,
                                    Disc = 0m,
                                    TaxId = mItem?.PurchaseTaxId.Value,
                                    TaxAmount = taxAmount,
                                    NettPrice = nettPrice,
                                    Total = itemDetail.Qty * nettPrice,
                                    Dpp = dpp,
                                    WarehouseCode = itemDetail.WarehouseCode,
                                    Type = itemDetail.Type
                                });
                            }
                        }
                    }

                    if (rcvHeadData.SrcTrans == 2)
                    {
                        var dataPR = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == rcvHeadData.TransCode);
                        if (dataPR.Type == 1)
                        {
                            // Update Debit Memo
                            var dbtMemo = Db.DebitMemos.FirstOrDefault(x => x.TransCode == rcvHeadData.TransCode);
                            var availableAmount = dbtMemo.Amount - dbtMemo.Used;
                            if (rcvHeadData.Total > availableAmount)
                            {
                                result.Message = "Tidak bisa disimpan karena jumlah total penerimaan pembeliam lebih besar dari jumlah total memo debit.";
                                return result;
                            }
                            availableAmount -= rcvHeadData.Total;
                            dbtMemo.Used += rcvHeadData.Total;
                            dbtMemo.Mark = availableAmount == 0 ? "FU" : "PU";
                            dbtMemo.UpdatedBy = rcvHeadData.UpdatedBy;
                            dbtMemo.UpdatedDate = rcvHeadData.UpdatedDate;
                            Db.DebitMemos.Update(dbtMemo);

                            // Update Purchase Return Detail
                            var prData = Db.PurchaseReturnDetails.Where(x => x.Code == rcvHeadData.TransCode).ToList();
                            foreach (var item in rcvDetailData)
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
                            var prhData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == rcvHeadData.TransCode);
                            prhData.Mark = dbtMemo.Mark == "A" ? "A" : (dbtMemo.Mark == "PU" ? "PR" : "CMP");
                            Db.PurchaseReturnHeaders.Update(prhData);
                        }
                    }

                    // Save changes
                    Db.SaveChanges();

                    itemData.RcvCode = rcvHeadData.Code;
                    Db.MobileReceiveItemHeaders.Update(itemData);

                    var rcvDetail = Db.PurchaseReceiveDetails.Where(x => x.Code == rcvHeadData.Code).ToList();

                    rcvHeadData.FinalDiscPercent = 0m;
                    rcvHeadData.FinalDisc = 0m;
                    rcvHeadData.IncludeTax = false;
                    rcvHeadData.TaxAmount = rcvDetail.Sum(x => x.Qty * x.TaxAmount);
                    rcvHeadData.Total = rcvDetail.Sum(x => x.Total);
                    rcvHeadData.Dpp = rcvDetail.Sum(x => x.Qty * x.Dpp);

                    Db.PurchaseReceiveHeaders.Update(rcvHeadData);

                    // Execute sp_update_stock_mutation_from_rcv
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                        rcvHeadData.Code, rcvHeadData.Date, rcvHeadData.TransCode);

                    if (itemData.SrcTrans == 1)
                    {
                        // Execute sp_update_po_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_po_rcv_qty {0}", rcvHeadData.TransCode);
                    }
                    else
                    {
                        // Execute sp_update_pr_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_pr_rcv_qty {0}", rcvHeadData.TransCode);
                    }

                    itemData.RcvCode = rcvHeadData.Code;
                    itemData.Mark = "APR";
                    itemData.ApprovedBy = userId;
                    itemData.ApprovedDate = rcvHeadData.ApprovedDate;
                    Db.MobileReceiveItemHeaders.Update(itemData);
                }

                // Save changes
                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Data penerimaan barang mobile berhasil disetujui.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobileReceiveItemHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.ReceiveInitial.Contains(search) || x.SupInitial.StartsWith(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public IEnumerable<VwMobileReceiveItemDetail> GetDetailData(string code)
        {
            var data = Db.VwMobileReceiveItemDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.LineNo);
        }

        public SaveResult Reject(List<MobileReceiveItemHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

            foreach (var item in data)
            {

                var rcvData = Db.MobileReceiveItemHeaders.FirstOrDefault(x => x.Code == item.Code);
                rcvData.RejectedBy = userId;
                rcvData.RejectedDate = DateTime.Now;
                rcvData.Mark = "REJ";
                Db.MobileReceiveItemHeaders.Update(rcvData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data penerimaan barang mobile berhasil ditolak.";
            return result;
        }

        public SaveResult Update(MobileReceiveItemRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get detail data that exists in receive before
                var delDetails = Db.MobileReceiveItemDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in receive before
                Db.MobileReceiveItemDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.MobileReceiveItemDetails.Add(new MobileReceiveItemDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            TransDetailId = item.TransDetailId,
                            ItemId = item.ItemId,
                            Qty = item.Qty,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            WarehouseCode = item.WarehouseCode,
                            Type = item.Type
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.MobileReceiveItemDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                        Db.Entry(item).Property(e => e.TransDetailId).IsModified = false;
                    }
                }

                // Save changes
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
            result.Message = "Data penerimaan barang mobile berhasil diperbarui.";
            return result;
        }
    }
}
