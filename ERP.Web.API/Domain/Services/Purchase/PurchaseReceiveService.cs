using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model.Purchase;

namespace ERP.Web.API.Domain.Services.Purchase;

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
            where piD.Contains(piH.Code) && piH.Mark != "V"
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
            if (data.SrcTrans == 2)
            {
                var transData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data retur pembelian tidak ditemukan.";
                    return result;
                }

                // Checking purchase return mark
                if (transData.Mark == "V")
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data retur pembelian sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase return date with purchase receive
                if (transData.Date > data.Date)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data retur pembelian mempunyai tanggal lebih besar.";
                    return result;
                }
            }
            else
            {
                var transData = Db.PurchaseOrderHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data order pembelian tidak ditemukan.";
                    return result;
                }

                // Checking purchase order mark
                if (transData.Mark is "V")
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data order pembelian sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase order date with purchase receive
                if (transData.Date > data.Date)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa disimpan karena data order pembelian mempunyai tanggal lebih besar.";
                    return result;
                }

                if (IsSaveAndDateInvalid(data))
                {
                    result.Message = "Tanggal Faktur Pembelian tidak boleh lebih kecil dari tanggal Penerimaan Barang.";
                    return result;
                }
            }
                
            // Checking receive qty is excess or not
            if (IsQtyExcess(data.SrcTrans, data.TransCode, data.ItemDetails, null))
            {
                result.Message = "Data penerimaan pembelian tidak bisa diubah karena qty yg diterima lebih besar dari qty yang tersedia.";
                return result;
            }

            var taxes = Db.Taxes.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();

            // Get new code
            var newCode = GetNewCode("RCV_NUM_FMT", data.Date);
                    
            data.Code = newCode;

            // Insert detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                if (data.SrcTrans == 1)
                {
                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);
                    if (data.IncludeTax)
                    {
                        item.TaxAmount = taxData != null ? item.Type  == 1 ? 0m : (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.Rate / 100))) : 0m;
                        item.ExemptTaxAmount = taxData != null ? item.Type  == 1 ? 0m : (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.ExemptRate / 100))) : 0m;
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = taxData != null ? item.Type == 1 ? 0m : (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.Rate / 100) : 0m;
                        item.ExemptTaxAmount = taxData != null ? item.Type == 1 ? 0m : (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.ExemptRate / 100) : 0m;
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                    }

                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalTax.Add(item.TaxAmount != 0 ? item.Qty * item.TaxAmount : 0m);
                    totalExemptTax.Add(item.ExemptTaxAmount != 0 ? item.Qty * item.ExemptTaxAmount : 0m);
                    totalDpp.Add(item.Qty * item.Dpp);
                }

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
                    FinalDiscHeader = item.FinalDiscHeader,
                    TaxId = item.TaxId,
                    TaxAmount = item.TaxAmount,
                    ExemptTaxAmount = item.ExemptTaxAmount,
                    NettPrice = item.NettPrice,
                    Total = item.Total,
                    Dpp = item.Dpp,
                    WarehouseCode = item.WarehouseCode,
                    Type = item.Type
                });
            }

            if (data.SrcTrans == 1)
            {
                data.SubTotal = totalDetail.Sum();
                data.TaxAmount = totalTax.Sum();
                data.ExemptTaxAmount = totalExemptTax.Sum();
                data.Dpp = totalDpp.Sum();
                data.Total = data.SubTotal;
            }
            Db.PurchaseReceiveHeaders.Add(data);

            if (data.IsPoInv)
            {
                // Purchase Invoice
                var newInvCode = GetNewCode("PI_NUM_FMT", data.Date);
                var newPinvData = new PurchaseInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.InvDate,
                    DueDate = data.InvDueDate,
                    PoCode = data.TransCode,
                    RefNo = data.InvRefNo,
                    SupCode = data.SupCode,
                    CurrCode = data.CurrCode,
                    //PaidAmount = data.Total,
                    Total = data.Total,
                    //Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.PurchaseInvoiceHeaders.Add(newPinvData);

                Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                {
                    Code = newInvCode,
                    LineNo = 1,
                    RcvCode = newCode,
                    SubTotal = data.Total,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });
            }

            // Save changes
            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_rcv
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                data.Code, data.Date, data.TransCode);

            // Execute sp_update_po_rcv_qty / sp_update_pr_rcv_qty
            Db.Database.ExecuteSqlRaw(
                data.SrcTrans == 1 ? "EXEC sp_update_po_rcv_qty {0}" : "EXEC sp_update_pr_rcv_qty {0}",
                data.TransCode);

            if (data.IsPoInv)
            {
                // Update purchase receive to invoiced
                Db.Database.ExecuteSqlRaw(
                    "UPDATE Purchasing.PurchaseReceiveHeader SET Mark='INV' WHERE Code={0}", data.Code);

                // Update purchase order to closed if all purchase receive are invoiced
                //if (
                //    !Db.PurchaseReceiveHeaders
                //        .Any(x => x.TransCode == data.TransCode && x.Mark != "INV"))
                //{
                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.TransCode);
                //}
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

            if (data.SrcTrans == 2)
            {
                var transData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data retur pembelian tidak ditemukan.";
                    return result;
                }

                // Checking purchase return mark
                if (transData.Mark == "V")
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data retur pembelian sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase return date with purchase receive
                if (transData.Date > data.Date)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data retur pembelian mempunyai tanggal lebih besar.";
                    return result;
                }
            }
            else
            {
                var transData = Db.PurchaseOrderHeaders.FirstOrDefault(x => x.Code == data.TransCode);

                if (transData == null)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data order pembelian tidak ditemukan.";
                    return result;
                }

                // Checking purchase order mark
                if (transData.Mark is "V")
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data order pembelian sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase order date with purchase receive
                if (transData.Date > data.Date)
                {
                    result.Message = "Data penerimaan pembelian tidak bisa diubah karena data order pembelian mempunyai tanggal lebih besar.";
                    return result;
                }

                if (IsSaveAndDateInvalid(data))
                {
                    result.Message = "Tanggal Faktur Pembelian tidak boleh lebih kecil dari tanggal Penerimaan Barang.";
                    return result;
                }
            }

            // Checking receive qty is excess or not
            if (IsQtyExcess(data.SrcTrans, data.TransCode, data.ItemDetails, data.Code))
            {
                result.Message = "Data penerimaan pembelian tidak bisa diubah karena qty yg diterima lebih besar dari qty yang tersedia.";
                return result;
            }

            //update Data if changed TransCode
            var oldRcvData = Db.PurchaseReceiveHeaders.AsNoTracking().FirstOrDefault(x => x.Code == data.Code);
            if (oldRcvData.TransCode != data.TransCode)
            {
                RestorePrevData(oldRcvData.Code, oldRcvData.TransCode, oldRcvData.SrcTrans);
            }

            //Restore stock mutation
            RestoreWarehouseQty(oldRcvData.Code, oldRcvData.TransCode, oldRcvData.SrcTrans);

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            var taxes = Db.Taxes.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();

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
                if (data.SrcTrans == 1)
                {
                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);

                    if (data.IncludeTax)
                    {
                        item.TaxAmount = taxData != null ? (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.Rate / 100))) : 0m;
                        item.ExemptTaxAmount = taxData != null ? (item.UnitPrice - item.Disc - item.FinalDiscHeader) - ((item.UnitPrice - item.Disc - item.FinalDiscHeader) / (1 + (taxData.ExemptRate / 100))) : 0m;
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = taxData != null ? (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.Rate / 100) : 0m;
                        item.ExemptTaxAmount = taxData != null ? (item.UnitPrice - item.Disc - item.FinalDiscHeader) * (taxData.ExemptRate / 100) : 0m;
                        item.NettPrice = item.UnitPrice - item.Disc - item.FinalDiscHeader + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - item.FinalDiscHeader;
                    }

                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalTax.Add(item.Qty * item.TaxAmount);
                    totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                    totalDpp.Add(item.Qty * item.Dpp);
                }

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
                        FinalDiscHeader = item.FinalDiscHeader,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        ExemptTaxAmount = item.ExemptTaxAmount,
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

            if (data.SrcTrans == 1)
            {
                data.SubTotal = totalDetail.Sum();
                data.TaxAmount = totalTax.Sum();
                data.ExemptTaxAmount = totalExemptTax.Sum();
                data.Dpp = totalDpp.Sum();
                data.Total = data.SubTotal;
            }
            // Update header data
            Db.PurchaseReceiveHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            if (data.IsPoInv)
            {
                // Purchase Invoice
                var newInvCode = GetNewCode("PI_NUM_FMT", data.Date);
                var newPinvData = new PurchaseInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.InvDate,
                    DueDate = data.InvDueDate,
                    PoCode = data.TransCode,
                    RefNo = data.InvRefNo,
                    SupCode = data.SupCode,
                    CurrCode = data.CurrCode,
                    //PaidAmount = data.Total,
                    Total = data.Total,
                    //Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.PurchaseInvoiceHeaders.Add(newPinvData);

                Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                {
                    Code = newInvCode,
                    LineNo = 1,
                    RcvCode = data.Code,
                    SubTotal = data.Total,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });
            }

            // Save changes
            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_rcv
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}",
                data.Code, data.Date, data.TransCode);

            // Execute sp_update_po_rcv_qty / sp_update_pr_rcv_qty
            Db.Database.ExecuteSqlRaw(
                data.SrcTrans == 1 ? "EXEC sp_update_po_rcv_qty {0}" : "EXEC sp_update_pr_rcv_qty {0}",
                data.TransCode);

            if (data.IsPoInv)
            {
                // Update purchase receive to invoiced
                Db.Database.ExecuteSqlRaw(
                    "UPDATE Purchasing.PurchaseReceiveHeader SET Mark='INV' WHERE Code={0}", data.Code);

                // Check all purchase receive are invoiced
                //if (
                //    !Db.PurchaseReceiveHeaders
                //        .Any(x => x.TransCode == data.TransCode && x.Mark != "INV"))
                //{
                //    // Update purchase order to closed
                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.TransCode);
                //}
                //else
                //{
                //    // Update purchase order to partial receive or completed
                //    var poMark = Db.PurchaseOrderDetails.Any(x => x.Code == data.TransCode && x.Qty > x.QtyRcv)
                //        ? "PR"
                //        : "CMP";

                //    Db.Database.ExecuteSqlRaw(
                //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark={0} WHERE Code={1}", poMark, data.TransCode);
                //}
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
                    
                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_rcv {0}, {1}, {2}, {3}",
                    data.Code, data.Date, data.TransCode, true);

                // Execute sp_update_po_rcv_qty / sp_update_pr_rcv_qty
                Db.Database.ExecuteSqlRaw(
                    data.SrcTrans == 1 ? "EXEC sp_update_po_rcv_qty {0}" : "EXEC sp_update_pr_rcv_qty {0}",
                    data.TransCode);

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
        
    private bool IsQtyExcess(int srcTrans, string transCode, IEnumerable<PurchaseReceiveDetail> items, string code)
    {
        var result = false;
        if (srcTrans == 1) // Purchase Order
        {
            foreach (var item in items)
            {
                if (item.Type == 0)
                {
                    var dataPODetail = Db.PurchaseOrderDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId && x.Type == 0);
                    if (code == null)
                    {
                        var availableStock = dataPODetail.Qty - dataPODetail.QtyRcv;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                    else if (dataPODetail != null)
                    {
                        var oldPRD = Db.PurchaseReceiveDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                        var availableStock = dataPODetail.Qty - (dataPODetail.QtyRcv - oldPRD?.Qty ?? 0);
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                }
            }
        }
        else // Purchase Return
        {
            foreach (var item in items)
            {
                var prData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == transCode);
                var dataPRDetail = Db.PurchaseReturnDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId);

                if (code == null)
                {
                    var availableStock = dataPRDetail.Qty - dataPRDetail.QtyRcv;
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
                else if (dataPRDetail != null)
                {
                    var oldPRD = Db.PurchaseReceiveDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                    var availableStock = (dataPRDetail.Qty) - (dataPRDetail.QtyRcv - oldPRD?.Qty ?? 0);
                    if (item.Qty > availableStock)
                    {
                        result = true;
                    }
                }
            }
        }
        return result;
    }

    private bool IsSaveAndDateInvalid(PurchaseReceiveRequest data)
    {
        var result = false;

        if (data.IsPoInv && data.InvDate < data.Date)
            result = true;

        return result;
    }

    private void RestorePrevData(string code, string transCode, int srcTrans)
    {
        var detailRcvData = Db.PurchaseReceiveDetails.AsNoTracking().Where(x => x.Code == code).ToList();
        if (srcTrans == 1)
        {
            var PoData = Db.PurchaseOrderHeaders.FirstOrDefault(x => x.Code == transCode);
            var detailPoData = Db.PurchaseOrderDetails.Where(x => x.Code == transCode).ToList();
            foreach (var item in detailPoData)
            {
                if (detailRcvData.Where(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Any())
                    item.QtyRcv -= detailRcvData.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Qty;
            }
            Db.PurchaseOrderDetails.UpdateRange(detailPoData);
            PoData.Mark = detailPoData.Sum(x => x.QtyRcv) == 0 ? "A" : detailPoData.Sum(x => x.QtyRcv) == detailPoData.Sum(x => x.Qty) ? "CMP" : "PR";
            Db.PurchaseOrderHeaders.Update(PoData);
        }
        else
        {
            var PrData = Db.PurchaseReturnHeaders.FirstOrDefault(x => x.Code == transCode);
            var detailPrData = Db.PurchaseReturnDetails.Where(x => x.Code == transCode).ToList();
            foreach (var item in detailPrData)
            {
                if (detailRcvData.Where(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Any())
                    item.QtyRcv -= detailRcvData.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId).Qty;
            }
            Db.PurchaseReturnDetails.UpdateRange(detailPrData);
            PrData.Mark = detailPrData.Sum(x => x.QtyRcv) == 0 ? "A" : detailPrData.Sum(x => x.QtyRcv) == detailPrData.Sum(x => x.Qty) ? "CMP" : "PR";
            Db.PurchaseReturnHeaders.Update(PrData);
        }
        Db.SaveChanges();
    }

    private void RestoreWarehouseQty(string code, string srcCode, short srcTrans)
    {
        var rcvSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code).ToList();
        var transSMData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == srcCode).ToList();
        if (srcTrans == 1)
        {
            foreach (var itemData in rcvSMData)
            {
                var whQtyData = new Entity.Inventory.WarehouseQuantity();
                if (itemData.Type == "OH")
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnHand = whQtyData.QtyOnHand - itemData.BaseQty;
                }
                else if (itemData.Type == "OI")
                {
                    var itemTransSMData = transSMData.FirstOrDefault(x => x.ItemId == itemData.ItemId && x.UnitId == itemData.UnitId);
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemTransSMData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnIndent = whQtyData.QtyOnIndent + itemData.BaseQty;
                }
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        else
        {
            foreach (var itemData in rcvSMData)
            {
                var whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                if (itemData.Type == "OH")
                    whQtyData.QtyOnHand = whQtyData.QtyOnHand - itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        Db.SaveChanges();
    }
}