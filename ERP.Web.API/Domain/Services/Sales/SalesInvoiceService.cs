using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class SalesInvoiceService : GeneralService<SalesInvoiceHeader>, ISalesInvoiceService
    {
        public SalesInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSalesInvoiceHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustCode.StartsWith(search) || x.CustName.Contains(search) ||
                        x.SoCode == search || x.IssuedInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<SalesInvoiceDetail> GetDetailData(string code)
        {
            return Db.SalesInvoiceDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {

            var data = (from h in Db.GeneralCashBankHeaders
                        join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                        where h.Mark != "V" && d.TransCode == code
                        select new
                        {
                            h.Code,
                            h.Date,
                            h.Amount
                        });

            return data.ToDynamicList();
        }
        public List<dynamic> GetDataMemo(string code)
        {

            var data = (from h in Db.SalesInvoiceCreditMemos
                        join d in Db.CreditMemos on h.CreditMemoCode equals d.Code
                        where h.InvCode == code
                        select new
                        {
                            h.Id,
                            CreditMemoCode = d.Code,
                            d.Date,
                            Type = d.SrcTrans,
                            CreditMemoAmount = h.CreditMemoAmount
                        }).Union(from h in Db.SalesInvoiceCreditMemos
                                 join d in Db.BeginningBalanceCreditMemos on h.CreditMemoCode equals d.Code
                                 where h.InvCode == code
                                 select new
                                 {
                                     h.Id,
                                     CreditMemoCode = d.Code,
                                     d.Date,
                                     d.Type,
                                     CreditMemoAmount = h.CreditMemoAmount
                                 });

            return data.ToDynamicList();
        }
        public SaveResult Insert(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking sales order mark
                if (IsSalesOrderInvalid(data.SoCode))
                {
                    result.Message = "Data faktur penjualan tidak bisa disimpan karena status order penjualan bukan diterima sebagian atau selesai.";
                    return result;
                }

                // Checking sales delivery mark & date
                if (
                    Db.SalesDeliveryHeaders.Any(d =>
                        data.Details.Select(i => i.DoCode).Contains(d.Code) &&
                        (d.Mark == "V" || d.Date > data.Date)))
                {
                    result.Message = "Data faktur penjualan tidak bisa disimpan karena status surat jalan sudah ditandai sebagai void atau mempunyai tanggal lebih besar dari faktur.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("SI_NUM_FMT", data.Date);
                    
                // Insert header data
                data.Code = newCode;
                data.PaidAmount = data.Memos.Sum(x => x.CreditMemoAmount);
                Db.SalesInvoiceHeaders.Add(data);

                // Decrease CreditUsed
                UpdateCreditUsed(data.CustCode, data.PaidAmount);

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        DoCode = item.DoCode,
                        ShipmentFee = item.ShipmentFee,
                        HandlingFee = item.HandlingFee,
                        SubTotal = item.SubTotal,
                        FinalDisc = item.FinalDisc,
                        TaxAmount = item.TaxAmount,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                // insert memo
                foreach (var item in data.Memos)
                {
                    Db.SalesInvoiceCreditMemos.Add(new SalesInvoiceCreditMemo
                    {
                        InvCode = newCode,
                        InvAmount = data.Total,
                        CreditMemoAmount = item.CreditMemoAmount,
                        CreditMemoCode = item.CreditMemoCode
                    });
                }
                // Save changes
                Db.SaveChanges();

                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.DoCode.Replace("'", "''")))}')");

                // Update sales order to closed if all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
                }

                UpdateCreditMemo(data);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data faktur penjualan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark != "A"))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena status data bukan aktif.";
                    return result;
                }

                // Checking sales order mark
                if (IsSalesOrderInvalid(data.SoCode))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena status order penjualan bukan diterima sebagian atau selesai.";
                    return result;
                }

                // Checking sales delivery mark & date
                if (
                    Db.SalesDeliveryHeaders.Any(d =>
                        data.Details.Select(i => i.DoCode).Contains(d.Code) &&
                        (d.Mark == "V" || d.Date > data.Date)))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena status surat jalan sudah ditandai sebagai void atau mempunyai tanggal lebih besar dari faktur.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Restore CreditUsed
                RestoreCreditUsed(data.Code, data.CustCode);

                // Update header data
                Db.SalesInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Restore Credit Memo
                RestoreCreditMemo(data.Code);

                // Get delivery code that exists in invoice before
                var doCodeList = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.DoCode).Contains(d.DoCode))
                    .Select(x => x.DoCode)
                    .ToList();

                // Update sales delivery mark that exists in invoice before
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='A'
                    WHERE Code IN ('{string.Join("','", doCodeList)}')");

                // Get detail data that exists in invoice before
                var delDetails = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in invoice before
                var delMemos = Db.SalesInvoiceCreditMemos
                    .Where(d => d.InvCode == data.Code && !data.Memos.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in invoice before
                Db.SalesInvoiceDetails.RemoveRange(delDetails);
                
                // Delete memo data that exists in invoice before
                Db.SalesInvoiceCreditMemos.RemoveRange(delMemos);

                // Update detail data
                short i = 0;
                var newInvDetails = new List<SalesInvoiceDetail>();
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        newInvDetails.Add(new SalesInvoiceDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            DoCode = item.DoCode,
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

                        Db.SalesInvoiceDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                var newMemos = new List<SalesInvoiceCreditMemo>();
                foreach (var item in data.Memos)
                {
                    newMemos.Add(new SalesInvoiceCreditMemo
                    {
                        InvCode = data.Code,
                        InvAmount = data.Total,
                        CreditMemoAmount = item.CreditMemoAmount,
                        CreditMemoCode = item.CreditMemoCode
                    });
                }

                // Insert detail if new data exists
                if (newInvDetails.Any())
                    Db.SalesInvoiceDetails.AddRange(newInvDetails);

                // Insert detail if new data exists
                if (newMemos.Any())
                    Db.SalesInvoiceCreditMemos.AddRange(newMemos);

                // Save changes
                Db.SaveChanges();

                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.DoCode.Replace("'", "''")))}')");

                // Check all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
                {
                    // Update sales order to closed
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
                }
                else
                {
                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);
                }

                // Decrease CreditUsed
                UpdateCreditUsed(data.CustCode, data.PaidAmount);
                
                UpdateCreditMemo(data);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data faktur penjualan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            if (IsAlreadyInTransaction(code))
            {
                result.Message = "Data faktur penjualan tidak bisa ditandai sebagai void karena sudah ada di transaksi kas bank.";
                return result;
            }

            var data = Db.SalesInvoiceHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data faktur penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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

                    // Get sales delivery code and join
                    var doCodeJoin = string.Join("','",
                        Db.SalesInvoiceDetails
                            .Where(x => x.Code == code)
                            .Select(x => x.DoCode.Replace("'", "''")));

                    // Update sales delivery to active
                    Db.Database.ExecuteSqlRaw(
                        $"UPDATE Sales.SalesDeliveryHeader SET Mark='A' WHERE Code IN ('{doCodeJoin}')");

                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);

                    // Restore Credit Memo
                    RestoreCreditMemo(code);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data faktur penjualan berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsSalesOrderInvalid(string soCode)
        {
            return Db.SalesOrderHeaders.Any(x => x.Code == soCode && !new[] { "PS", "CMP" }.Contains(x.Mark));
        }

        private bool IsAlreadyInTransaction(string code)
        {
            return (from h in Db.GeneralCashBankHeaders
                    join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                    where h.Mark == "A" && d.TransCode == code
                    select h.Code).Any();
        }

        #region Credit Used - Limit
        private void RestoreCreditUsed(string transCode, string custCode)
        {
            var prevAmount = Db.SalesInvoiceHeaders.AsNoTracking().FirstOrDefault(x => x.Code.Equals(transCode))?.Total;
            string query = $"update General.Customer set CreditUsed= (CreditUsed + {prevAmount}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        private void UpdateCreditUsed(string custCode, decimal total)
        {
            string query = $"update General.Customer set CreditUsed= (CreditUsed - {total}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        #endregion

        #region Update & Restore Credit Memo
        private void UpdateCreditMemo(SalesInvoiceRequest data)
        {
            var listQuery = new List<string>();
            if (data.Memos.Any())
            {
                var listCodeMemo = data.Memos.Select(x => x.CreditMemoCode).ToList();
                var listCreditMemo = GetListCreditMemo(listCodeMemo);
                foreach (var item in data.Memos)
                {
                    var selectedMemo = listCreditMemo.FirstOrDefault(x => x.Code.Equals(item.CreditMemoCode));
                    if (selectedMemo != null)
                    {
                        decimal used = selectedMemo.Used + item.CreditMemoAmount;
                        string status = used == selectedMemo.Amount ? "FU" : "PU";
                        string query = selectedMemo.Source == "cm" ? $"update Sales.CreditMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceCreditMemo set Used = {used} where code = '{selectedMemo.Code}'";
                        listQuery.Add(query);
                    }
                }
                ExecuteQuery(listQuery);
            }
        }
        private void RestoreCreditMemo(string code)
        {
            var listQuery = new List<string>();
            var usedMemo = Db.SalesInvoiceCreditMemos.Where(x => x.InvCode.Equals(code)).ToList();
            if (usedMemo.Any())
            {
                var listCodeMemo = usedMemo.Select(x => x.CreditMemoCode).ToList();
                var listCreditMemo = GetListCreditMemo(listCodeMemo);
                foreach (var item in usedMemo)
                {
                    var selectedMemo = listCreditMemo.FirstOrDefault(x => x.Code.Equals(item.CreditMemoCode));
                    if (selectedMemo != null)
                    {
                        decimal used = selectedMemo.Used - item.CreditMemoAmount;
                        string status = used > 0 ? "PU" : "A";
                        string query = selectedMemo.Source == "cm" ? $"update Sales.CreditMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceCreditMemo set Used = {used} where code = '{selectedMemo.Code}'";
                        listQuery.Add(query);
                    }
                }
                ExecuteQuery(listQuery);
            }
        }
        private List<dynamic> GetListCreditMemo(List<string> listCodeMemo)
        {
            return (from cm in Db.CreditMemos
                    where listCodeMemo.Contains(cm.Code)
                    select new { cm.Code, cm.Amount, cm.Used, Source = "cm" })
                        .Union
                        (from cm in Db.BeginningBalanceCreditMemos
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
}
