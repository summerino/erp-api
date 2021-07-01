using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Services.Purchase
{
    public class PurchaseInvoiceService : GeneralService<PurchaseInvoiceHeader>, IPurchaseInvoiceService
    {
        public PurchaseInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwPurchaseInvoiceHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SupName.Contains(search) || x.PoCode == search ||
                        x.IssuedInitial.Contains(search) || x.RefNo.StartsWith(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<PurchaseInvoiceDetail> GetDetailData(string code)
        {
            return Db.PurchaseInvoiceDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(PurchaseInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.PoCode))
                {
                    result.Message = "Data faktur pembelian tidak bisa diubah karena status order pembelian bukan diterima sebagian atau selesai.";
                    return result;
                }
                
                // Get new code
                var newCode = GetNewCode("PI_NUM_FMT", data.Date);
                    
                // Insert header data
                data.Code = newCode;
                Db.PurchaseInvoiceHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.PurchaseInvoiceDetails.Add(new PurchaseInvoiceDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        RcvCode = item.RcvCode,
                        ShipmentFee = item.ShipmentFee,
                        HandlingFee = item.HandlingFee,
                        SubTotal = item.SubTotal,
                        FinalDisc = item.FinalDisc,
                        TaxAmount = item.TaxAmount,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                // Save changes
                Db.SaveChanges();

                // Update purchase receive to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Purchasing.PurchaseReceiveHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.RcvCode.Replace("'", "''")))}')");

                // Update purchase order to closed if all purchase receive are invoiced
                if (
                    !Db.PurchaseReceiveHeaders
                        .Any(x => x.TransCode == data.PoCode && x.Mark != "INV"))
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.PoCode);
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
            result.Message = "Data faktur pembelian berhasil disimpan.";
            return result;
        }

        public SaveResult Update(PurchaseInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.PurchaseInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data faktur pembelian tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                // Checking purchase order mark
                if (IsPurchaseOrderInvalid(data.PoCode))
                {
                    result.Message = "Data faktur pembelian tidak bisa diubah karena status order pembelian bukan diterima sebagian atau selesai.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.PurchaseInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get receive code that exists in invoice before
                var rcvCodeList = Db.PurchaseInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.RcvCode).Contains(d.RcvCode))
                    .Select(x => x.RcvCode)
                    .ToList();

                // Update purchase receive mark that exists in invoice before
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Purchasing.PurchaseReceiveHeader
                    SET Mark='A'
                    WHERE Code IN ('{string.Join("','", rcvCodeList)}')");

                // Get detail data that exists in invoice before
                var delDetails = Db.PurchaseInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in invoice before
                Db.PurchaseInvoiceDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                var newInvDetails = new List<PurchaseInvoiceDetail>();
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        newInvDetails.Add(new PurchaseInvoiceDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            RcvCode = item.RcvCode,
                            ShipmentFee = item.ShipmentFee,
                            HandlingFee = item.HandlingFee,
                            SubTotal = item.SubTotal,
                            FinalDisc = item.FinalDisc,
                            TaxAmount = item.TaxAmount,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PurchaseInvoiceDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Insert detail if new data exists
                if (newInvDetails.Any())
                    Db.PurchaseInvoiceDetails.AddRange(newInvDetails);

                // Save changes
                Db.SaveChanges();

                // Update purchase receive to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Purchasing.PurchaseReceiveHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.RcvCode.Replace("'", "''")))}')");

                // Check all purchase receive are invoiced
                if (
                    !Db.PurchaseReceiveHeaders
                        .Any(x => x.TransCode == data.PoCode && x.Mark != "INV"))
                {
                    // Update purchase order to closed
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.PoCode);
                }
                else
                {
                    // Update purchase order to partial receive or completed
                    var poMark = Db.PurchaseOrderDetails.Any(x => x.Code == data.PoCode && x.Qty > x.QtyRcv)
                        ? "PR"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Purchasing.PurchaseOrderHeader SET Mark={0} WHERE Code={1}", poMark, data.PoCode);
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
            result.Message = "Data faktur pembelian berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PurchaseInvoiceHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data faktur pembelian tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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

                    // Get purchase receive code and join
                    var rcvCodeJoin = string.Join("','",
                        Db.PurchaseInvoiceDetails
                            .Where(x => x.Code == code)
                            .Select(x => x.RcvCode.Replace("'", "''")));

                    // Update purchase receive to active
                    Db.Database.ExecuteSqlRaw(
                        $"UPDATE Purchasing.PurchaseReceiveHeader SET Mark='A' WHERE Code IN ('{rcvCodeJoin}')");

                    // Update purchase order to partial receive or completed
                    var poMark = Db.PurchaseOrderDetails.Any(x => x.Code == data.PoCode && x.Qty > x.QtyRcv)
                        ? "PR"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Purchasing.PurchaseOrderHeader SET Mark={0} WHERE Code={1}", poMark, data.PoCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data faktur pembelian berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsPurchaseOrderInvalid(string poCode)
        {
            return Db.PurchaseOrderHeaders.Any(x => x.Code == poCode && !new[] { "PR", "CMP" }.Contains(x.Mark));
        }
    }
}
