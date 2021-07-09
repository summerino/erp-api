using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Finance;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Finance;
using ERP_API.Domain.Models;
using ERP_API.Model.Finance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP_API.Domain.Services.Finance
{
    public class CashBankService : GeneralService<CashBankRequest>, ICashBankService
    {
        public CashBankService(TenantContext db)
            : base(db)
        {
        }
        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwGeneralCashBankHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.VouCode.Contains(search) || x.Type.Contains(search) ||
                        x.CurrCode == search || x.ChequeNo.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            throw new NotImplementedException();
        }

        public DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            throw new NotImplementedException();
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

                var querys = new List<string>();
                (result.Message, result.Success, querys) = Validate(data);
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
                        Notes = item.Notes
                    });
                    j++;
                }

                if (querys.Any()) {
                    string query = Convert(querys);
                    if (!string.IsNullOrEmpty(query))
                    {
                        Db.Database.ExecuteSqlRaw(query);
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
            result.Message = "Data bank tunai berhasil disimpan.";
            return result;
        }

        public SaveResult Update(CashBankRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var querys = new List<string>();
                (result.Message, result.Success, querys) = Validate(data);
                if (!result.Success) return result;

                // Checking mark header data
                if (Db.GeneralCashBankHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data bank tunai tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

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
                            Notes = item.Notes
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;
                        Db.GeneralCashBankDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                if (querys.Any())
                {
                    string query = Convert(querys);
                    if (!string.IsNullOrEmpty(query))
                    {
                        Db.Database.ExecuteSqlRaw(query);
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
            result.Message = "Data bank tunai berhasil diperbarui.";
            return result;
        }
        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.GeneralCashBankHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data bank tunai tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data bank tunai berhasil ditandai sebagai void.";
            return result;
        }

        private (string, bool, List<string>) Validate(CashBankRequest data) 
        {
            var listTransCode = data.ItemDetails.Select(x => x.TransCode).ToList();
            
            // validasi hutang
            var oldTransactions = (from h in Db.GeneralCashBankHeaders
                                  join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                                  where h.Mark == "A" && listTransCode.Contains(d.TransCode) && h.Code != data.Code
                                  select d).ToList();
            
            var querys = new List<string>();
            
            foreach (var item in data.ItemDetails)
            {
                decimal prevTransaction = oldTransactions.Where(x => x.TransCode.Equals(item.TransCode)).Sum(x => x.Amount);
                decimal totalAmount = prevTransaction + item.Amount;

                if (item.Type == "AP") 
                {
                    //validasi hutang
                    var header = Db.PurchaseInvoiceHeaders.SingleOrDefault(x => x.Code.Equals(item.TransCode));

                    if (totalAmount <= header.Total)
                    {
                        string queryHeader = $"UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount='{totalAmount}' WHERE Code='{item.TransCode}';";
                        querys.Add(queryHeader);

                        var detail = Db.PurchaseInvoiceDetails.Where(x => x.Code.Equals(item.TransCode));
                        foreach (var item2 in detail)
                        {
                            var proRateValue = totalAmount * item2.Total / header.Total;
                            string queryDetail = $"UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount='{proRateValue}' WHERE Code='{item2.RcvCode}';";
                            querys.Add(queryDetail);
                        }
                    }
                    else 
                    {
                        return ($"Lebih bayar untuk transaksi dengan kode {header.Code}.", false, new List<string>());
                    }
                } 
                else if (item.Type == "AR") 
                {
                    // validasi piutang
                    var header = Db.SalesInvoiceHeaders.SingleOrDefault(x => x.Code.Equals(item.TransCode));
                    
                    if (totalAmount <= header.Total)
                    {
                        string queryHeader = $"UPDATE Sales.SalesInvoiceHeader SET PaidAmount='{totalAmount}' WHERE Code='{item.TransCode}';";
                        querys.Add(queryHeader);

                        var detail = Db.SalesInvoiceDetails.Where(x => x.Code.Equals(item.TransCode));
                        foreach (var item2 in detail)
                        {
                            var proRateValue = totalAmount * item2.Total / header.Total;
                            string queryDetail = $"UPDATE Sales.SalesDeliveryHeader SET PaidAmount='{proRateValue}' WHERE Code='{item2.DoCode}';";
                            querys.Add(queryDetail);
                        }
                    }
                    else
                    {
                        return ($"Lebih bayar untuk transaksi dengan kode {header.Code}.", false, new List<string>());
                    }
                }
                else if (item.Type == "DPC" || item.Type == "DPS") 
                {
                    string query = QueryBuilder(item.Type, item.TransCode);
                    querys.Add(query);
                }
                else if (item.Type == "PR" || item.Type == "RDPS" || item.Type == "RDPC" || item.Type == "SR")
                {
                    // validasi retur uang muka pembelian dan retur pembelian
                    if (item.Type == "PR" || item.Type == "RDPS")
                    {
                        var memo = Db.DebitMemos.SingleOrDefault(x => x.Code.Equals(item.TransCode));
                        if (memo != null) 
                        {
                            decimal remaining = memo.Amount - memo.Used;
                            if (totalAmount > remaining) 
                            {
                                return ($"Lebih bayar untuk transaksi dengan kode {memo.Code}.", false, new List<string>());
                            }
                        }
                    }
                    else if(item.Type == "RDPC" || item.Type == "SR")
                    {
                        var memo = Db.CreditMemos.SingleOrDefault(x => x.Code.Equals(item.TransCode));
                        if (memo != null)
                        {
                            decimal remaining = memo.Amount - memo.Used;
                            if (totalAmount > remaining)
                            {
                                return ($"Lebih bayar untuk transaksi dengan kode {memo.Code}.", false, new List<string>());
                            }
                        }
                    }
                    string query = QueryBuilder(item.Type, item.TransCode, totalAmount);
                    querys.Add(query);
                }
            }
            return ("", true, querys);
        }
       
        private string QueryBuilder(string type, string code, decimal amount = 0) 
        {
            string query = "";
             if (type == "DPC")
            {
                query = $"UPDATE Sales.CreditMemo SET Mark='A' WHERE Code='{code}';";
            }
            else if (type == "DPS")
            {
                query = $"UPDATE Purchasing.DebitMemo SET Mark='A' WHERE Code='{code}';";
            }
            else if (type == "PR" || type == "RDPS")
            {
                query = $"UPDATE Purchasing.DebitMemo SET Used='{amount}' WHERE Code='{code}';";
            }
            else if (type == "RDPC" || type == "SR")
            {
                query = $"UPDATE Sales.CreditMemo SET Used='{amount}' WHERE Code='{code}';";
            }
            return query;
        }
        private string Convert(List<string> querys) 
        {
            string query = "";
            foreach (var item in querys)
            {
                query += item;
            }
            return query;
        }
    }
}
