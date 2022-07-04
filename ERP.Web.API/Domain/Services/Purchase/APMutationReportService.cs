using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase;

public class APMutationReportService : IAPMutationReportService
{
    private readonly TenantContext _db;
    public APMutationReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string startDate, string endDate, string supCode, string status)
    {
        var supData = _db.ReportBySupplierMutations.FromSqlRaw(@"SELECT sp.Code, sp.Name, COUNT(*) AS TotalTrans, CAST (0 AS decimal) AS BeginningBalance, SUM(inv.Total) AS TransAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS EndingBalance
                        FROM General.Supplier sp
                        LEFT JOIN Purchasing.PurchaseInvoiceHeader inv on inv.SupCode = sp.Code
                        WHERE inv.Mark IN('A', 'PP', 'CMP') GROUP BY sp.Code, sp.Initial, sp.Name").ToList();

        var invData = _db.ReportByInvoiceAPMutations.FromSqlRaw(@"SELECT inv.Date, inv.DueDate, inv.Code, inv.POCode AS OrderCode, inv.SupCode, sp.[Name] AS SupName, CAST (0 AS decimal) AS BeginningBalance, inv.Total AS TransAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS EndingBalance
                                    FROM Purchasing.PurchaseInvoiceHeader inv
                                    LEFT JOIN General.Supplier sp on sp.Code = inv.SupCode
                                    WHERE inv.Mark IN('A', 'PP', 'CMP')").ToList();

        var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V").ToList();

        var bbData = _db.VwBeginningBalanceAPs.Where(x => x.IsActive).ToList();

        var invDMData = _db.PurchaseInvoiceDebitMemos.Where(x => invData.Select(y => y.Code).Contains(x.InvCode)).ToList();

        var dmData = _db.DebitMemos.Where(x => x.Mark != "V").ToList();

        if (!string.IsNullOrEmpty(endDate))
        {
            invData = invData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            cbData = cbData.Where(x => (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(endDate)).ToList();
            bbData = bbData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            dmData = dmData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
        }

        var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

        // [b] prefix for before - [c] prefix for current
        foreach (var itemInv in invData)
        {
            if (!string.IsNullOrEmpty(startDate))
            {
                if (itemInv.Date < Convert.ToDateTime(startDate))
                {
                    var bcbData = cbData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
                    var totBcb = cbDetail.Where(x => x.TransCode == itemInv.Code && bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && !bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    var totBdm = dmData.Where(x => x.Date < Convert.ToDateTime(startDate) && invDMData.Select(y => y.DebitMemoCode).Contains(x.Code)).Sum(x => x.Amount);
                    var totCdm = dmData.Where(x => x.Date >= Convert.ToDateTime(startDate) && invDMData.Select(y => y.DebitMemoCode).Contains(x.Code)).Sum(x => x.Amount);
                    itemInv.BeginningBalance = itemInv.TransAmount - totBcb - totBdm;
                    itemInv.PaidAmount = totCcb + totCdm;
                    itemInv.EndingBalance = itemInv.BeginningBalance - itemInv.PaidAmount;
                    itemInv.TransAmount = 0;
                }
                else
                {
                    var ccbData = cbData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                    var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && ccbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    var totCdm = dmData.Where(x => x.Date >= Convert.ToDateTime(startDate) && invDMData.Select(y => y.DebitMemoCode).Contains(x.Code)).Sum(x => x.Amount);
                    itemInv.PaidAmount = totCcb + totCdm;
                    itemInv.EndingBalance = itemInv.TransAmount - itemInv.PaidAmount;
                }
            }
            else
            {
                var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && cbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                var totCdm = dmData.Where(x => invDMData.Select(y => y.DebitMemoCode).Contains(x.Code)).Sum(x => x.Amount);
                itemInv.PaidAmount = totCcb + totCdm;
                itemInv.EndingBalance = itemInv.TransAmount - itemInv.PaidAmount;
            }
        }

        foreach (var itemBB in bbData)
        {
            var newData = new ReportByInvoiceAPMutation
            {
                Date = itemBB.Date,
                DueDate = itemBB.DueDate,
                Code = itemBB.Code,
                OrderCode = "",
                SupCode = itemBB.SupCode,
                SupName = itemBB.SupName,
                BeginningBalance = 0m,
                TransAmount = itemBB.Amount,
                PaidAmount = 0m,
                EndingBalance = 0m
            };

            if (!string.IsNullOrEmpty(startDate))
            {
                if (itemBB.Date < Convert.ToDateTime(startDate))
                {
                    var bcbData = cbData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
                    var totBcb = cbDetail.Where(x => x.TransCode == newData.Code && bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    var totCcb = cbDetail.Where(x => x.TransCode == newData.Code && !bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    newData.BeginningBalance = newData.TransAmount - totBcb;
                    newData.PaidAmount = totCcb;
                    newData.EndingBalance = newData.BeginningBalance - newData.PaidAmount;
                    newData.TransAmount = 0;
                }
                else
                {
                    var ccbData = cbData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                    var totCcb = cbDetail.Where(x => x.TransCode == newData.Code && ccbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    newData.PaidAmount = totCcb;
                    newData.EndingBalance = newData.TransAmount - newData.PaidAmount;
                }
            }
            else
            {
                var totCcb = cbDetail.Where(x => x.TransCode == newData.Code && cbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                newData.PaidAmount = totCcb;
                newData.EndingBalance = newData.TransAmount - newData.PaidAmount;
            }

            invData.Add(newData);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "NP")
            {
                invData = invData.Where(x => x.EndingBalance > 0).ToList();
            }
            else if (status == "P")
            {
                invData = invData.Where(x => x.EndingBalance == 0).ToList();
            }
        }

        invData = invData
            .Where(x => x.BeginningBalance > 0 || x.TransAmount > 0 || x.PaidAmount > 0 || x.EndingBalance > 0)
            .OrderBy(x => x.Date).ToList();

        foreach (var itemSup in supData)
        {
            itemSup.TotalTrans = invData.Count(x => x.SupCode == itemSup.Code);
            itemSup.BeginningBalance = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.BeginningBalance);
            itemSup.TransAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TransAmount);
            itemSup.PaidAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.PaidAmount);
            itemSup.EndingBalance = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.EndingBalance);
        }

        supData = supData.Where(x => x.TotalTrans > 0).ToList();

        if (type == 1)
        {
            if (!string.IsNullOrEmpty(supCode))
            {
                invData = invData.Where(x => x.SupCode == supCode).ToList();
            }

            invData = invData.OrderBy(x => x.Date).ToList();

            invData.Add(new ReportByInvoiceAPMutation
            {
                Code = "Total",
                BeginningBalance = invData.Sum(x => x.BeginningBalance),
                TransAmount = invData.Sum(x => x.TransAmount),
                PaidAmount = invData.Sum(x => x.PaidAmount),
                EndingBalance = invData.Sum(x => x.EndingBalance),
            });

            return invData.AsQueryable().ToDataSourceResult(0, invData.Count, null, null);
        }
        else
        {
            if (!string.IsNullOrEmpty(supCode))
            {
                supData = supData.Where(x => x.Code == supCode).ToList();
            }

            supData.Add(new ReportBySupplierMutation
            {
                Name = "Total",
                BeginningBalance = supData.Sum(x => x.BeginningBalance),
                TransAmount = supData.Sum(x => x.TransAmount),
                PaidAmount = supData.Sum(x => x.PaidAmount),
                EndingBalance = supData.Sum(x => x.EndingBalance),
            });

            return supData.AsQueryable().ToDataSourceResult(0, supData.Count, null, null);
        }
    }
}