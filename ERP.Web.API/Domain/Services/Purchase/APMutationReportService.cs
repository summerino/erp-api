using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
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
            
            if (!string.IsNullOrEmpty(endDate))
            {
                invData = invData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                cbData = cbData.Where(x => (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(endDate)).ToList();
            }

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();


            foreach (var itemInv in invData)
            {
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (itemInv.Date < Convert.ToDateTime(startDate))
                    {
                        var bcbData = cbData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
                        var totBcb = cbDetail.Where(x => x.TransCode == itemInv.Code && bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && !bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        itemInv.BeginningBalance = itemInv.TransAmount - totBcb;
                        itemInv.PaidAmount = totCcb;
                        itemInv.EndingBalance = itemInv.BeginningBalance - itemInv.PaidAmount;
                        itemInv.TransAmount = 0;
                    }
                    else
                    {
                        var ccbData = cbData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                        var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && ccbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        itemInv.PaidAmount = totCcb;
                        itemInv.EndingBalance = itemInv.TransAmount - itemInv.PaidAmount;
                    }
                }
                else
                {
                    var totCcb = cbDetail.Where(x => x.TransCode == itemInv.Code && cbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    itemInv.PaidAmount = totCcb;
                    itemInv.EndingBalance = itemInv.TransAmount - itemInv.PaidAmount;
                }
            }

            foreach (var itemSup in supData)
            {
                itemSup.TotalTrans = invData.Count(x => x.SupCode == itemSup.Code);
                itemSup.BeginningBalance = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.BeginningBalance);
                itemSup.TransAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.TransAmount);
                itemSup.PaidAmount = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.PaidAmount);
                itemSup.EndingBalance = invData.Where(x => x.SupCode == itemSup.Code).Sum(x => x.EndingBalance);
            }

            supData = supData.Where(x => x.TotalTrans > 0).ToList();

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

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(supCode))
                {
                    invData = invData.Where(x => x.SupCode == supCode).ToList();
                }

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
                    BeginningBalance = invData.Sum(x => x.BeginningBalance),
                    TransAmount = invData.Sum(x => x.TransAmount),
                    PaidAmount = invData.Sum(x => x.PaidAmount),
                    EndingBalance = invData.Sum(x => x.EndingBalance),
                });

                return supData.AsQueryable().ToDataSourceResult(0, supData.Count, null, null);
            }
        }
    }
}
