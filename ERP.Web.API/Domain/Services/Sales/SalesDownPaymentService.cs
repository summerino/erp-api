using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesDownPaymentService : GeneralService<CreditMemo>, ISalesDownPaymentService
{
    public SalesDownPaymentService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwCreditMemos.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.CustName.Contains(search) || x.SrcTransName == search ||
                    x.TransCode.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public List<dynamic> GetRelatedTransactions(string code)
    {
        var data = (from h in Db.GeneralCashBankHeaders
            join d in Db.GeneralCashBankDetails on h.Code equals d.Code
            where h.Mark == "A" && d.TransCode == code
            select new
            {
                Code = h.Code,
                Date = h.Date,
                Total = h.Amount
            }).Union(
            from s in Db.SalesInvoiceCreditMemos
            join c in Db.SalesInvoiceHeaders on s.InvCode equals c.Code
            where s.CreditMemoCode == code
            select new
            { 
                Code = s.InvCode,
                Date = c.Date,
                Total = c.Total
            });
        return data.ToDynamicList();
    }

    public List<dynamic> GetReturnRelatedTransactions(string code)
    {
        var data = Db.CreditMemos.Where(x => x.TransCode == code).Select(x => new
        {
            Code = x.Code,
            Date = x.Date,
            Total = x.Amount
        });
        return data.ToDynamicList();
    }

    public override SaveResult Insert(CreditMemo data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {

            // Get new code
            var newCode = GetNewCode("SLS_DP_NUM_FMT", data.CreatedDate);

            // Insert data
            data.Code = newCode;
            Db.CreditMemos.Add(data);

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
        result.Message = "Data uang muka penjualan berhasil disimpan.";
        return result;
    }

    public override SaveResult Update(CreditMemo data)
    {
        var result = new SaveResult(false);

        // Update data
        Db.CreditMemos.Update(data);
        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.SrcTrans).IsModified = false;
        Db.Entry(data).Property(e => e.Used).IsModified = false;
        Db.Entry(data).Property(e => e.Mark).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data uang muka penjualan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.CreditMemos.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data uang muka penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Update header data
            data.Mark = "V";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data uang muka penjualan berhasil ditandai sebagai void.";
        return result;
    }
}