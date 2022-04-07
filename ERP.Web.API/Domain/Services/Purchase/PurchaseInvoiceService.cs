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

    public List<dynamic> GetDataMemo(string code)
    {
        var data = (from h in Db.PurchaseInvoiceDebitMemos
            join d in Db.DebitMemos on h.DebitMemoCode equals d.Code
            where h.InvCode == code
            select new
            {
                h.Id,
                DebitMemoCode = d.Code,
                d.Date,
                Type = d.SrcTrans,
                h.DebitMemoAmount
            }).Union(from h in Db.PurchaseInvoiceDebitMemos
            join d in Db.BeginningBalanceDebitMemos on h.DebitMemoCode equals d.Code
            where h.InvCode == code
            select new
            {
                h.Id,
                DebitMemoCode = d.Code,
                d.Date,
                d.Type,
                h.DebitMemoAmount
            });

        return data.ToDynamicList();
    }

    public List<dynamic> GetRelatedTransactions(string code)
    {
        var data = (from h in Db.GeneralCashBankHeaders
            join d in Db.GeneralCashBankDetails on h.Code equals d.Code
            where h.Mark == "A" && d.TransCode == code
            select new
            {
                h.Code,
                h.Date,
                h.Amount
            });

        return data.ToDynamicList();
    }

    public SaveResult Insert(PurchaseInvoiceRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking purchase order mark
            if (IsPurchaseOrderInvalid(data.PoCode) && data.Mark != "A")
            {
                result.Message = "Data faktur pembelian tidak bisa disimpan karena status order pembelian bukan diterima sebagian atau selesai.";
                return result;
            }

            // Checking purchase receive mark & date
            if (
                Db.PurchaseReceiveHeaders.Any(pr =>
                    data.Details.Select(i => i.RcvCode).Contains(pr.Code) &&
                    (pr.Mark != "A" || pr.Date > data.Date)))
            {
                result.Message = "Data faktur pembelian tidak bisa disimpan karena status penerimaan pembelian bukan aktif atau mempunyai tanggal lebih besar dari faktur.";
                return result;
            }

            // Get new code
            var newCode = GetNewCode("PI_NUM_FMT", data.Date);
                    
            // Insert header data
            data.Code = newCode;
            data.PaidAmount = data.Memos.Sum(x => x.DebitMemoAmount);
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

            foreach (var item in data.Memos)
            {
                Db.PurchaseInvoiceDebitMemos.Add(new PurchaseInvoiceDebitMemo
                {
                    InvCode = newCode,
                    InvAmount = data.Total,
                    DebitMemoAmount = item.DebitMemoAmount,
                    DebitMemoCode = item.DebitMemoCode
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
            //if (
            //    !Db.PurchaseReceiveHeaders
            //        .Any(x => x.TransCode == data.PoCode && x.Mark != "INV"))
            //{
            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.PoCode);
            //}
            UpdateDebitMemo(data);

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
            if (Db.PurchaseInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark != "A"))
            {
                result.Message = "Data faktur pembelian tidak bisa diubah karena status data bukan aktif.";
                return result;
            }

            // Checking purchase order mark
            if (IsPurchaseOrderInvalid(data.PoCode) && data.Mark != "A")
            {
                result.Message = "Data faktur pembelian tidak bisa diubah karena status order pembelian bukan diterima sebagian atau selesai.";
                return result;
            }

            // Checking purchase receive mark & date
            if (
                Db.PurchaseReceiveHeaders.Any(pr =>
                    data.Details.Select(i => i.RcvCode).Contains(pr.Code) &&
                    (pr.Mark != "A" && !Db.PurchaseInvoiceDetails
                         .Where(pi => pi.Code == data.Code)
                         .Select(pi => pi.RcvCode).Contains(pr.Code) ||
                     pr.Date > data.Date)))
            {
                result.Message = "Data faktur pembelian tidak bisa diubah karena status penerimaan pembelian sudah ditandai sebagai void atau mempunyai tanggal lebih besar dari faktur.";
                return result;
            }

            data.ApprovedBy = null;
            data.ApprovedDate = null;


            // Restore Debit Memo
            RestoreDebitMemo(data.Code);

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

            // Get detail data that exists in invoice before
            var delMemos = Db.PurchaseInvoiceDebitMemos
                .Where(d => d.InvCode == data.Code)
                .ToList();

            // Delete detail data that exists in invoice before
            Db.PurchaseInvoiceDetails.RemoveRange(delDetails);

            // Delete detail data that exists in invoice before
            Db.PurchaseInvoiceDebitMemos.RemoveRange(delMemos);

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

            var newMemos = new List<PurchaseInvoiceDebitMemo>();
            foreach (var item in data.Memos)
            {
                newMemos.Add(new PurchaseInvoiceDebitMemo
                {
                    InvCode = data.Code,
                    InvAmount = data.Total,
                    DebitMemoAmount = item.DebitMemoAmount,
                    DebitMemoCode = item.DebitMemoCode
                });
            }

            // Insert detail if new data exists
            if (newInvDetails.Any())
                Db.PurchaseInvoiceDetails.AddRange(newInvDetails);

            // Insert detail if new data exists
            if (newMemos.Any())
                Db.PurchaseInvoiceDebitMemos.AddRange(newMemos);

            // Update header data
            data.PaidAmount = newMemos.Any() ? newMemos.Sum(x => x.DebitMemoAmount) : 0;
            Db.PurchaseInvoiceHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Update Debit Memo
            UpdateDebitMemo(data);

            // Save changes
            Db.SaveChanges();

            // Update purchase receive to invoiced
            Db.Database.ExecuteSqlRaw(
                $@"UPDATE Purchasing.PurchaseReceiveHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.RcvCode.Replace("'", "''")))}')");

            // Check all purchase receive are invoiced
            //if (
            //    !Db.PurchaseReceiveHeaders
            //        .Any(x => x.TransCode == data.PoCode && x.Mark != "INV"))
            //{
            //    // Update purchase order to closed
            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.PoCode);
            //}
            //else
            //{
            //    // Update purchase order to partial receive or completed
            //    var poMark = Db.PurchaseOrderDetails.Any(x => x.Code == data.PoCode && x.Qty > x.QtyRcv)
            //        ? "PR"
            //        : "CMP";

            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Purchasing.PurchaseOrderHeader SET Mark={0} WHERE Code={1}", poMark, data.PoCode);
            //}

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

        if (IsAlreadyInTransaction(code))
        {
            result.Message = "Data faktur pembelian tidak bisa ditandai sebagai void karena sudah ada di transaksi kas bank.";
            return result;
        }

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

                // update credit memo
                RestoreDebitMemo(code);

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

    private bool IsAlreadyInTransaction(string code)
    {
        return (from h in Db.GeneralCashBankHeaders
            join d in Db.GeneralCashBankDetails on h.Code equals d.Code
            where h.Mark == "A" && d.TransCode == code
            select h.Code).Any();
    }

    #region Update & Restore Debit Memo
    private void UpdateDebitMemo(PurchaseInvoiceRequest data)
    {
        var listQuery = new List<string>();
        if (data.Memos.Any())
        {
            var listCodeMemo = data.Memos.Select(x => x.DebitMemoCode).ToList();
            var listDebitMemo = GetListDebitMemo(listCodeMemo);
            foreach (var item in data.Memos)
            {
                var selectedMemo = listDebitMemo.FirstOrDefault(x => x.Code.Equals(item.DebitMemoCode));
                if (selectedMemo != null)
                {
                    decimal used = selectedMemo.Used + item.DebitMemoAmount;
                    string status = used == selectedMemo.Amount ? "FU" : "PU";
                    string query = selectedMemo.Source == "dm" ? $"update Purchasing.DebitMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceDebitMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'";
                    listQuery.Add(query);
                }
            }
            ExecuteQuery(listQuery);
        }
    }
    private void RestoreDebitMemo(string code)
    {
        var listQuery = new List<string>();
        var usedMemo = Db.PurchaseInvoiceDebitMemos.Where(x => x.InvCode.Equals(code)).ToList();
        if (usedMemo.Any())
        {
            var listCodeMemo = usedMemo.Select(x => x.DebitMemoCode).ToList();
            var listDebitMemo = GetListDebitMemo(listCodeMemo);
            foreach (var item in usedMemo)
            {
                var selectedMemo = listDebitMemo.FirstOrDefault(x => x.Code.Equals(item.DebitMemoCode));
                if (selectedMemo != null)
                {
                    decimal used = selectedMemo.Used - item.DebitMemoAmount;
                    string status = used > 0 ? "PU" : "A";
                    string query = selectedMemo.Source == "dm" ? $"update Purchasing.DebitMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceDebitMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'";
                    listQuery.Add(query);
                }
            }
            ExecuteQuery(listQuery);
        }
    }
    private List<dynamic> GetListDebitMemo(List<string> listCodeMemo)
    {
        return (from cm in Db.DebitMemos
                where listCodeMemo.Contains(cm.Code)
                select new { cm.Code, cm.Amount, cm.Used, Source = "dm" })
            .Union
            (from cm in Db.BeginningBalanceDebitMemos
                where listCodeMemo.Contains(cm.Code)
                select new { cm.Code, cm.Amount, cm.Used, Source = "bb" }).ToList<dynamic>();
    }
    private void ExecuteQuery(List<string> listQuery)
    {
        if (listQuery.Any())
        {
            var querys = string.Join(";", listQuery);
            Db.Database.ExecuteSqlRaw(querys);
        }
    }
    #endregion
}