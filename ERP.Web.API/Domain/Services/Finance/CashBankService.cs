using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Models.Finance;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Services.Finance;

public class CashBankService : GeneralService<GeneralCashBankHeader>, ICashBankService
{
    public CashBankService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search)
    {
        var data = Db.VwGeneralCashBankHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.VouCode.Contains(search) || x.Type.Contains(search) ||
                    x.CoaCode.StartsWith(search) || x.CoaName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string cbCode)
    {
        var data = Db.VwARs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(cbCode))
        {
            var listExistingTransactions =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code.Equals(cbCode))
                    .Select(x => x.TransCode).ToList();
            data = data.Where(x => !listExistingTransactions.Contains(x.Code));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string cbCode)
    {
        var data = Db.VwAPs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(cbCode))
        {
            var listExistingTransactions =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code.Equals(cbCode))
                    .Select(x => x.TransCode).ToList();
            data = data.Where(x => !listExistingTransactions.Contains(x.Code));
        }
        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetDataEPAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string cbCode)
    {
        var data = Db.VwExpeditionInvoiceHeaders.Where(x => x.PaidAmount < x.Amount && x.Mark != "V");

        if (!string.IsNullOrWhiteSpace(cbCode))
        {
            var listExistingTransactions =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code.Equals(cbCode))
                    .Select(x => x.TransCode).ToList();
            data = data.Where(x => !listExistingTransactions.Contains(x.Code));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public DataSourceResult GetDataCreditMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string cbCode, string type)
    {
        var transLists = new List<string>();
        if (!string.IsNullOrWhiteSpace(cbCode))
        {
            transLists =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code.Equals(cbCode))
                    .Select(x => x.TransCode).ToList();
        }

        switch (type)
        {
            case "DPC":
            {
                var data = Db.VwCreditMemos.Where(x => x.SrcTrans == 1 && x.Mark == "PP");

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            case "RDPC":
            {
                //var data = (from bb in Db.BeginningBalanceCreditMemos
                //        join c in Db.Customers on bb.CustCode equals c.Code into cs
                //        from c in cs.DefaultIfEmpty()
                //        where bb.Type == 1 && bb.Used < bb.Amount && bb.IsActive
                //        select new
                //        {
                //            bb.Code, bb.Date, bb.CustCode, CustName = c != null ? c.Name : "",
                //            bb.CurrCode, bb.Rate,
                //            bb.Amount, bb.Used, Remaining = bb.Amount - bb.Used, bb.Notes, Src = "BB"
                //        })
                //    .Union(
                //        from cm in Db.VwCreditMemos
                //        where cm.SrcTrans == 1 && cm.Remaining > 0 && new[] { "A", "PU" }.Contains(cm.Mark)
                //        select new
                //        {
                //            cm.Code, cm.Date, cm.CustCode, cm.CustName,
                //            cm.CurrCode, Rate = 1m,
                //            cm.Amount, cm.Used, cm.Remaining, cm.Notes, Src = "CM"
                //        });

                var data = Db.VwOutstandingCreditMemos.Where(x => x.Type == 1);

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            case "SR":
            {
                var data = Db.VwOutstandingCreditMemos.Where(x => x.Type == 2);

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            default:
                return null;
        }
    }

    public DataSourceResult GetDataDebitMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search, string cbCode, string type)
    {
        var transLists = new List<string>();
        if (!string.IsNullOrWhiteSpace(cbCode))
        {
            transLists =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code.Equals(cbCode))
                    .Select(x => x.TransCode).ToList();
        }

        switch (type)
        {
            case "DPS":
            {
                var data = Db.VwDebitMemos.Where(x => x.SrcTrans == 1 && x.Mark == "PP");

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            case "RDPS":
            {
                var data = Db.VwOutstandingDebitMemos.Where(x => x.Type == 1);

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            case "PR":
            {
                var data = Db.VwOutstandingDebitMemos.Where(x => x.Type == 2);

                if (!string.IsNullOrWhiteSpace(cbCode))
                    data = data.Where(x => !transLists.Contains(x.Code));

                return data.ToDataSourceResult(skip, take, filters, sorts);
            }
            default:
                return null;
        }
    }

    public IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code)
    {
        var data = Db.VwGeneralCashBankDetails.Where(x => x.Code.Equals(code));
        return data;
    }

    public SaveResult Insert(CashBankRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            List<string> queries;
            (result.Message, result.Success, queries) = Validate(data);
            if (!result.Success) return result;

            // Get new code
            var newCode = GetNewCode("CB_NUM_FMT", data.Date);

            // Insert header data
            data.Code = newCode;
            Db.GeneralCashBankHeaders.Add(data);

            short j = 0;
            foreach (var item in data.ItemDetails)
            {
                Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                {
                    Code = data.Code,
                    LineNo = j,
                    Type = item.Type,
                    TransCode = item.TransCode,
                    CoaCode = item.CoaCode,
                    CurrCode = item.CurrCode,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    TypeAmount = item.TypeAmount,
                    TransAmount = item.TransAmount,
                    Notes = item.Notes,
                    Src = item.Src
                });
                j++;
            }


            if (queries.Any())
            {
                string query = string.Join("", queries);
                if (!string.IsNullOrEmpty(query))
                {
                    Db.Database.ExecuteSqlRaw(query);
                }
            }

            Db.SaveChanges();

            var listTransCodeAndAmount = data.ItemDetails.Select(x => new { x.TransCode, x.TransAmount }).ToList<dynamic>();
            UpdateCreditUsed(listTransCodeAndAmount);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data kas bank umum berhasil disimpan.";
        return result;
    }

    public SaveResult Update(CashBankRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var queries = new List<string>();
            (result.Message, result.Success, queries) = Validate(data);
            if (!result.Success) return result;

            // Checking mark header data
            if (Db.GeneralCashBankHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data kas bank umum tidak bisa diubah karena sudah ditandai sebagai void.";
                return result;
            }

            if (Db.GeneralCashBankHeaders.Any(x => x.Code == data.Code && x.Mark == "REJ"))
            {
                result.Message = "Data kas bank umum tidak bisa diubah karena sudah ditolak.";
                return result;
            }

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            // Restore Credit Used
            RestoreCreditUsed(data.Code);

            // Restore transaction to precious data. (roll back data menjadi ketika sebelum edit)
            Db.Database.ExecuteSqlRaw($"sp_restore_cash_bank_transaction '{data.Code}';");

            // Update header data
            Db.GeneralCashBankHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Get detail data that exists in order before
            var delDetails = Db.GeneralCashBankDetails
                .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Delete detail data that exists in order before
            Db.GeneralCashBankDetails.RemoveRange(delDetails);

            // Update detail data
            short i = 0;
            var listTransCodeAndTransAmount = new List<dynamic>();
            foreach (var item in data.ItemDetails)
            {
                if (item.Id <= 0)
                {
                    Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        Type = item.Type,
                        TransCode = item.TransCode,
                        CoaCode = item.CoaCode,
                        CurrCode = item.CurrCode,
                        Rate = item.Rate,
                        Amount = item.Amount,
                        TypeAmount = item.TypeAmount,
                        TransAmount = item.TransAmount,
                        Notes = item.Notes,
                        Src = item.Src
                    });
                }
                else
                {
                    item.LineNo = ++i;
                    Db.GeneralCashBankDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;
                    Db.Entry(item).Property(e => e.Type).IsModified = false;
                    Db.Entry(item).Property(e => e.Src).IsModified = false;

                }
                listTransCodeAndTransAmount.Add(new { item.TransCode, item.TransAmount });
            }

            if (queries.Any())
            {
                var query = string.Join("", queries);
                if (!string.IsNullOrEmpty(query))
                {
                    Db.Database.ExecuteSqlRaw(query);
                }
            }

            Db.SaveChanges();

            UpdateCreditUsed(listTransCodeAndTransAmount);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data kas bank umum berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId, int menuId, int roleId)
    {
        var result = new SaveResult(false);

        var data = Db.GeneralCashBankHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data kas bank umum tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Checking role authorization for item details
            var map = new MapCbTypeToAction();
            var types =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code == code)
                    .Select(x => x.Type).Distinct().ToList();
            var actionIdLists =
                map.CbTypeToActions
                    .Where(t => types.Contains(t.Code))
                    .Select(t => t.ActionId).ToList();

            var countRoleMenuAction =
                Db.RoleMenuActions
                    .Count(x => x.MenuId == menuId && x.RoleId == roleId && actionIdLists.Contains(x.ActionId));

            if (countRoleMenuAction != actionIdLists.Count)
            {
                result.Message = AppConstant.UnAuthMessage;
                return result;
            }

            // restore cash bank transaction
            Db.Database.ExecuteSqlRaw($"sp_restore_cash_bank_transaction '{code}';");

            // restore credit used customer
            RestoreCreditUsed(code);

            // Update header data
            data.Mark = "V";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data kas bank umum berhasil ditandai sebagai void.";
        return result;
    }


    public SaveResult Reject(string code, int userId, int menuId, int roleId)
    {
        var result = new SaveResult(false);

        var data = Db.GeneralCashBankHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (new[] { "V", "REJ" }.Contains(data.Mark))
            {
                result.Message = "Data kas bank umum tidak bisa ditolak karena status tidak aktif.";
                return result;
            }

            // Checking role authorization for item details
            var map = new MapCbTypeToAction();
            var types =
                Db.GeneralCashBankDetails
                    .Where(x => x.Code == code)
                    .Select(x => x.Type).Distinct().ToList();
            var actionIdLists =
                map.CbTypeToActions
                    .Where(t => types.Contains(t.Code))
                    .Select(t => t.ActionId).ToList();

            var countRoleMenuAction =
                Db.RoleMenuActions
                    .Count(x => x.MenuId == menuId && x.RoleId == roleId && actionIdLists.Contains(x.ActionId));

            if (countRoleMenuAction != actionIdLists.Count)
            {
                result.Message = AppConstant.UnAuthMessage;
                return result;
            }

            // restore cash bank transaction
            Db.Database.ExecuteSqlRaw($"sp_restore_cash_bank_transaction '{code}';");

            // restore credit used customer
            RestoreCreditUsed(code);

            // Update header data
            data.Mark = "REJ";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data kas bank umum berhasil ditolak.";
        return result;
    }

    private (string, bool, List<string>) Validate(CashBankRequest data)
    {
        var listTransCode = data.ItemDetails.Select(x => x.TransCode).ToList();
        var oldTransactions = Db.VwDebitCreditPayments.Where(x => listTransCode.Contains(x.TransCode) && x.Code != data.Code).ToList();

        var queries = new List<string>();

        foreach (var item in data.ItemDetails)
        {
            if (item.Type == "AR")
            {
                var prevTransaction = oldTransactions.Where(x => x.TransCode == item.TransCode).Sum(x => x.Amount);
                var totalAmount = prevTransaction + item.Amount;

                // Validasi piutang
                if (item.Src == "BB")
                {
                    var bbData = Db.BeginningBalanceARs.SingleOrDefault(x => x.Code == item.TransCode);

                    if (bbData == null)
                        continue;

                    if (data.ChequeDate.GetValueOrDefault(data.Date) < bbData.Date)
                        return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {bbData.Code}.", false, new List<string>());

                    if (totalAmount > bbData.Amount)
                        return ($"Lebih bayar untuk transaksi dengan kode {bbData.Code}.", false, new List<string>());

                    queries.Add(
                        $"UPDATE Accounting.BeginningBalanceAR SET PaidAmount='{totalAmount}' WHERE Code='{item.TransCode}';");
                }
                else
                {
                    var header = Db.SalesInvoiceHeaders.SingleOrDefault(x => x.Code == item.TransCode);

                    if (header == null)
                        continue;

                    if (data.ChequeDate.GetValueOrDefault(data.Date) < header.Date)
                        return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {header.Code}.", false, new List<string>());

                    if (totalAmount > header.Total)
                        return ($"Lebih bayar untuk transaksi dengan kode {header.Code}.", false, new List<string>());

                    var mark = header.Total == totalAmount ? "CMP" : "PP";

                    queries.Add(
                        $"UPDATE Sales.SalesInvoiceHeader SET PaidAmount='{totalAmount}', Mark='{mark}' WHERE Code='{item.TransCode}';");

                    var detail = Db.SalesInvoiceDetails.Where(x => x.Code == item.TransCode);
                    foreach (var item2 in detail)
                    {
                        var proRateValue = totalAmount * item2.Total / header.Total;
                        queries.Add(
                            $"UPDATE Sales.SalesDeliveryHeader SET PaidAmount='{proRateValue}' WHERE Code='{item2.DoCode}';");
                    }
                }
            }
            else if (item.Type == "AP")
            {
                var prevTransaction = oldTransactions.Where(x => x.TransCode == item.TransCode).Sum(x => x.Amount);
                var totalAmount = prevTransaction + item.Amount;

                // Validasi hutang
                if (item.Src == "BB")
                {
                    var bbData = Db.BeginningBalanceAPs.SingleOrDefault(x => x.Code == item.TransCode);

                    if (bbData == null)
                        continue;

                    if (data.ChequeDate.GetValueOrDefault(data.Date) < bbData.Date)
                        return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {bbData.Code}.", false, new List<string>());

                    if (totalAmount > bbData.Amount)
                        return ($"Lebih bayar untuk transaksi dengan kode {bbData.Code}.", false, new List<string>());

                    queries.Add(
                        $"UPDATE Accounting.BeginningBalanceAP SET PaidAmount='{totalAmount}' WHERE Code='{item.TransCode}';");
                }
                else
                {
                    var header = Db.PurchaseInvoiceHeaders.SingleOrDefault(x => x.Code == item.TransCode);
                        
                    if (header == null)
                        continue;

                    if (data.ChequeDate.GetValueOrDefault(data.Date) < header.Date)
                        return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {header.Code}.", false, new List<string>());

                    if (totalAmount > header.Total)
                        return ($"Lebih bayar untuk transaksi dengan kode {header.Code}.", false, new List<string>());

                    var mark = header.Total == totalAmount ? "CMP" : "PP";

                    queries.Add(
                        $"UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount='{totalAmount}', Mark='{mark}' WHERE Code='{item.TransCode}';");

                    var detail = Db.PurchaseInvoiceDetails.Where(x => x.Code == item.TransCode);
                    foreach (var item2 in detail)
                    {
                        var proRateValue = totalAmount * item2.Total / header.Total;
                        queries.Add(
                            $"UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount='{proRateValue}' WHERE Code='{item2.RcvCode}';");
                    }
                }
            }
            else if (item.Type == "EPAP")
            {
                var prevTransaction = oldTransactions.Where(x => x.TransCode == item.TransCode).Sum(x => x.Amount);
                var totalAmount = prevTransaction + item.Amount;

                // Validasi hutang ekspedisi
                var ep = Db.ExpeditionInvoiceHeaders.SingleOrDefault(x => x.Code == item.TransCode);

                if (ep == null)
                    continue;

                if (data.ChequeDate.GetValueOrDefault(data.Date) < ep.Date)
                    return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {ep.Code}.", false, new List<string>());

                if (totalAmount > ep.Amount)
                    return ($"Lebih bayar untuk transaksi dengan kode {ep.Code}.", false, new List<string>());

                var mark = ep.Amount == totalAmount ? "CMP" : "PP";

                queries.Add(
                    $"UPDATE Expedition.ExpeditionInvoiceHeader SET PaidAmount='{totalAmount}', Mark='{mark}' WHERE Code='{item.TransCode}';");
            }
            else if (item.Type == "DPC")
            {
                var memo = Db.CreditMemos.SingleOrDefault(x => x.Code == item.TransCode);

                if (memo == null)
                    continue;

                if (data.ChequeDate.GetValueOrDefault(data.Date) < memo.Date)
                    return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {memo.Code}.", false, new List<string>());

                var query = QueryBuilder(item.Type, item.TransCode);
                queries.Add(query);
            }
            else if (item.Type == "DPS")
            {
                var memo = Db.DebitMemos.SingleOrDefault(x => x.Code == item.TransCode);

                if (memo == null)
                    continue;

                if (data.ChequeDate.GetValueOrDefault(data.Date) < memo.Date)
                    return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {memo.Code}.", false, new List<string>());

                var query = QueryBuilder(item.Type, item.TransCode);
                queries.Add(query);
            }
            else if (item.Type is "PR" or "RDPS" or "RDPC" or "SR")
            {
                // Validasi retur uang muka pembelian dan retur pembelian
                if (item.Type is "RDPS" or "PR")
                {
                    var prevTransaction = oldTransactions.Where(x => x.TransCode == item.TransCode && x.TypeAmount == "C").Sum(x => x.Amount);
                    var totalAmount = prevTransaction + item.Amount;

                    if (item.Src == "BB")
                    {
                        var bbData = Db.BeginningBalanceDebitMemos.SingleOrDefault(x => x.Code.Equals(item.TransCode));

                        if (bbData == null)
                            continue;

                        if (data.ChequeDate.GetValueOrDefault(data.Date) < bbData.Date)
                            return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {bbData.Code}.", false, new List<string>());

                        if (totalAmount > bbData.Amount)
                            return ($"Lebih bayar untuk transaksi dengan kode {bbData.Code}.", false, new List<string>());

                        queries.Add(
                            $"UPDATE Accounting.BeginningBalanceDebitMemo SET Used='{totalAmount}' WHERE Code='{item.TransCode}';");
                    }
                    else
                    {
                        var memo = Db.DebitMemos.SingleOrDefault(x => x.Code == item.TransCode);
                            
                        if (memo == null)
                            continue;

                        if (data.ChequeDate.GetValueOrDefault(data.Date) < memo.Date)
                            return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {memo.Code}.", false, new List<string>());

                        if (totalAmount > memo.Amount)
                            return ($"Lebih bayar untuk transaksi dengan kode {memo.Code}.", false, new List<string>());

                        var mark = totalAmount == memo.Amount ? "FU" : "PU";

                        queries.Add(
                            $"UPDATE Purchasing.DebitMemo SET Used='{totalAmount}', Mark='{mark}' WHERE Code='{item.TransCode}';");
                    }
                }
                else if (item.Type is "RDPC" or "SR")
                {
                    var prevTransaction = oldTransactions.Where(x => x.TransCode == item.TransCode && x.TypeAmount == "D").Sum(x => x.Amount);
                    var totalAmount = prevTransaction + item.Amount;

                    if (item.Src == "BB")
                    {
                        var bbData = Db.BeginningBalanceCreditMemos.SingleOrDefault(x => x.Code == item.TransCode);

                        if (bbData == null)
                            continue;

                        if (data.ChequeDate.GetValueOrDefault(data.Date) < bbData.Date)
                            return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {bbData.Code}.", false, new List<string>());

                        if (totalAmount > bbData.Amount)
                            return ($"Lebih bayar untuk transaksi dengan kode {bbData.Code}.", false, new List<string>());

                        queries.Add(
                            $"UPDATE Accounting.BeginningBalanceCreditMemo SET Used='{totalAmount}' WHERE Code='{item.TransCode}';");
                    }
                    else
                    {
                        var memo = Db.CreditMemos.SingleOrDefault(x => x.Code == item.TransCode);

                        if (memo == null)
                            continue;

                        if (data.ChequeDate.GetValueOrDefault(data.Date) < memo.Date)
                            return ($"Tanggal kas bank tidak boleh lebih kecil dari tanggal transaksi {memo.Code}.", false, new List<string>());

                        if (totalAmount > memo.Amount)
                            return ($"Lebih bayar untuk transaksi dengan kode {memo.Code}.", false, new List<string>());

                        var mark = totalAmount == memo.Amount ? "FU" : "PU";

                        queries.Add(
                            $"UPDATE Sales.CreditMemo SET Used='{totalAmount}', Mark='{mark}' WHERE Code='{item.TransCode}';");
                    }
                }
            }
        }

        return ("", true, queries);
    }

    private string QueryBuilder(string type, string code, decimal amount = 0, string mark = "")
    {
        return type switch
        {
            "DPC" => $"UPDATE Sales.CreditMemo SET Mark='A' WHERE Code='{code}';",
            "DPS" => $"UPDATE Purchasing.DebitMemo SET Mark='A' WHERE Code='{code}';",
            _ => ""
        };
    }


    #region Credit Used - Limit
    private void RestoreCreditUsed(string cashBankCode)
    {
        var listTransactions = (from cd in Db.GeneralCashBankDetails
            join si in Db.SalesInvoiceHeaders on cd.TransCode equals si.Code
            join so in Db.SalesOrderHeaders on si.SoCode equals so.Code
            where cd.Code == cashBankCode
            select new { so.CustCode , cd.TransAmount }).ToList();
        var listQuery = new List<string>();
        foreach (var item in listTransactions)
        {
            string query = $"update General.Customer set CreditUsed= (CreditUsed + {item.TransAmount}) where code = '{item.CustCode}'";
            listQuery.Add(query);
        }
        if (listQuery.Any())
        {
            Db.Database.ExecuteSqlRaw(string.Join(";", listQuery));
        }
    }
    private void UpdateCreditUsed(List<dynamic> listTransCodeAndTransAmount)
    {
        var listTransCodeOnly = listTransCodeAndTransAmount.Select(x => x.TransCode).ToList();
        var listTransactions = (from so in Db.SalesOrderHeaders
            join si in Db.SalesInvoiceHeaders on so.Code equals si.SoCode
            where listTransCodeOnly.Contains(si.Code)
            select new { si.Code, so.CustCode }).ToList();

        var listQuery = new List<string>();
        foreach (var item in listTransCodeAndTransAmount)
        {
            string custCode = listTransactions.SingleOrDefault(x => x.Code.Equals(item.TransCode))?.CustCode;
            if (custCode != null) {
                string query = $"update General.Customer set CreditUsed= (CreditUsed - {item.TransAmount}) where code = '{custCode}'";
                listQuery.Add(query);
            }
                
        }
        if (listQuery.Any()) { 
            Db.Database.ExecuteSqlRaw(string.Join(";",listQuery));
        }
    }
    #endregion
}