using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Expedition;
using ERP.Web.API.Domain.Interfaces.Expedition;
using ERP.Web.API.Model.Expedition;

namespace ERP.Web.API.Domain.Services.Expedition;

public class ExpeditionInvoiceService : GeneralService<ExpeditionInvoiceHeader>, IExpeditionInvoiceService
{
    public ExpeditionInvoiceService(TenantContext db)
        :base(db)
    {
    }
       
    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwExpeditionInvoiceHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.RefNo.Contains(search) || x.SupInitial.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<dynamic> GetDetailData(string code)
    {
        var headerData = Db.ExpeditionInvoiceHeaders.FirstOrDefault(x => x.Code == code);
        List<dynamic> data;

        if (headerData.SrcTrans == 1)
        {
            data = (from dt in Db.ExpeditionInvoiceDetails
                    join dto in Db.PurchaseReceiveHeaders on dt.TransCode equals dto.Code
                    where dt.Code == code
                    select new
                    {
                        dt.Id,
                        dt.TransCode,
                        dt.LineNo,
                        dto.Date,
                        dto.Mark

                    }).ToList<dynamic>();
        }
        else
        {
            data = (from dt in Db.ExpeditionInvoiceDetails
                    join dto in Db.SalesDeliveryHeaders on dt.TransCode equals dto.Code
                    where dt.Code == code
                    select new
                    {
                        dt.Id,
                        dt.TransCode,
                        dt.LineNo,
                        dto.Date,
                        dto.Mark

                    }).ToList<dynamic>();
        }

        return data.OrderBy(x => x.LineNo).ToDynamicList();
    }

    public List<dynamic> GetRelatedTransactions(string code)
    {
        var data = (from h in Db.GeneralCashBankHeaders
            join d in Db.GeneralCashBankDetails on h.Code equals d.Code
            where h.Mark == "A" && d.TransCode == code && d.Type == "EPAP"
            select new
            {
                h.Code,
                h.Date,
                h.Amount
            });

        return data.ToDynamicList();
    }

    public SaveResult Insert(ExpeditionInvoiceRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Get new code
            var newCode = GetNewCode("EI_NUM_FMT", data.Date);

            // Insert header data
            data.Code = newCode;
            Db.ExpeditionInvoiceHeaders.Add(data);

            // Insert detail data
            short i = 0;
            foreach (var item in data.Details)
            {
                Db.ExpeditionInvoiceDetails.Add(new ExpeditionInvoiceDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    TransCode = item.TransCode
                });
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
        result.Message = "Data faktur ekspedisi berhasil disimpan.";
        return result;
    }

    public SaveResult Update(ExpeditionInvoiceRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking mark header data
            if (Db.ExpeditionInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data faktur ekspedisi tidak bisa diubah karena data sudah ditandai sebagai void.";
                return result;
            }

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            // Update header data
            Db.ExpeditionInvoiceHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.PaidAmount).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Get detail data that exists in invoice before
            var delDetails = Db.ExpeditionInvoiceDetails
                .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Get detail data that exists in invoice before
            Db.ExpeditionInvoiceDetails.RemoveRange(delDetails);

            // Update detail data
            short i = 0;
            foreach (var item in data.Details)
            {
                if (item.Id <= 0)
                {
                    Db.ExpeditionInvoiceDetails.Add(new ExpeditionInvoiceDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        TransCode = item.TransCode
                    });
                }
                else
                {
                    item.LineNo = ++i;

                    Db.ExpeditionInvoiceDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;
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
        result.Message = "Data faktur ekspedisi berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.ExpeditionInvoiceHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data faktur ekspedisi tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
        }

        result.Success = true;
        result.Message = "Data faktur ekspedisi berhasil ditandai sebagai void.";
        return result;
    }

    public DataSourceResult GetReceivesData(IEnumerable<Filter> filter)
    {
        var data = (from dt in Db.VwPurchaseReceiveHeaders
                    where dt.Mark != "V" &&
                    (
                        !(from eid in Db.ExpeditionInvoiceDetails 
                        join eih in Db.ExpeditionInvoiceHeaders on eid.Code equals eih.Code
                        where eih.Mark != "V"
                        select eid.TransCode).Contains(dt.Code)
                    )
                    select new
                    {
                        dt.Code,
                        dt.Date,
                        dt.SupName,
                        dt.Mark
                    }).AsQueryable();

        return data.ToDataSourceResult(0, data.Count(), filter, null);
    }

    public DataSourceResult GetDeliveriesData(IEnumerable<Filter> filter)
    {
        var data = (from dt in Db.VwSalesDeliveryHeaders
                    join dto in Db.VwSalesOrderHeaders on dt.TransCode equals dto.Code into dtos
                    from dtosRes in dtos.DefaultIfEmpty()
                    join dtr in Db.VwSalesReturnHeaders on dt.TransCode equals dtr.Code into dtrs
                    from dtrsRes in dtrs.DefaultIfEmpty()
                    where dt.Mark != "V" &&
                    (
                        !(from eid in Db.ExpeditionInvoiceDetails
                          join eih in Db.ExpeditionInvoiceHeaders on eid.Code equals eih.Code
                          where eih.Mark != "V"
                          select eid.TransCode).Contains(dt.Code)
                    )
                    select new
                    {
                        dt.Code,
                        dt.Date,
                        dt.CustName,
                        SalesName = dt.SrcTrans == 1 ? dtosRes.SalesName : dtrsRes.SalesName,
                        dt.Mark
                    }).AsQueryable();

        return data.ToDataSourceResult(0, data.Count(), filter, null);
    }
}