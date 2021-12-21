using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class ARMutationReportService : IARMutationReportService
    {
        private readonly TenantContext _db;
        public ARMutationReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int type, string startDate, string endDate, string custCode, int slsId, string status)
        {
            var cusData = _db.ReportByCustomerMutations.FromSqlRaw(@"SELECT cs.Code, cs.Initial, cs.[Name], COUNT(*) AS TotalTrans, CAST (0 AS decimal) AS BeginningBalance, SUM(inv.Total) AS TransAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS EndingBalance
                            FROM General.Customer cs
                            LEFT JOIN Sales.SalesDeliveryHeader dlv ON dlv.CustCode = cs.Code
                            LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                            LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A', 'PP', 'CMP')
                            WHERE dlv.Mark IN('A', 'INV') AND inv.Total IS NOT NULL GROUP BY cs.Code, cs.Initial, cs.[Name]").ToList();

            var dlvData = _db.ReportByDeliveryARMutations.FromSqlRaw(@"SELECT dlv.[Date], inv.DueDate, dlv.Code, dlv.TransCode AS SrcCode, inv.Code AS InvCode, sls.FirstName AS SlsName, dlv.CustCode, sp.[Name] AS CustName, CAST (0 AS decimal) AS BeginningBalance, dlv.Total AS TransAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS EndingBalance
                            FROM Sales.SalesDeliveryHeader dlv
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = dlv.TransCode
                            LEFT JOIN General.Employee sls ON sls.Id = so.SalesBy
                            LEFT JOIN General.Customer sp ON sp.Code = dlv.CustCode
                            LEFT JOIN Sales.SalesInvoiceDetail invD ON invD.DOCode = dlv.Code
                            LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = invD.Code AND inv.Mark IN('A','PP','CMP')
                            WHERE dlv.Mark IN('A','INV')" + (slsId > 0 ? $" AND so.SalesBy = {slsId} " : " ") + "").ToList();

            var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V").ToList();

            var bbData = _db.VwBeginningBalanceARs.Where(x => x.IsActive).ToList();

            if (!string.IsNullOrEmpty(endDate))
            {
                dlvData = dlvData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
                cbData = cbData.Where(x => (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(endDate)).ToList();
                bbData = bbData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
            }

            var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

            foreach (var item in dlvData)
            {
                if (!string.IsNullOrEmpty(startDate))
                {
                    var totDlv = _db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == item.InvCode)?.Total ?? 0m;
                    if (item.Date < Convert.ToDateTime(startDate))
                    {
                        var bcbData = cbData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
                        var totBcb = cbDetail.Where(x => x.TransCode == item.InvCode && bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        var totCcb = cbDetail.Where(x => x.TransCode == item.InvCode && !bcbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        item.BeginningBalance = item.TransAmount - (totDlv > 0 ? (totBcb * item.TransAmount / totDlv) : 0m);
                        item.PaidAmount = totDlv > 0 ? (totCcb * item.TransAmount / totDlv) : 0m;
                        item.EndingBalance = item.BeginningBalance - item.PaidAmount;
                        item.TransAmount = 0;
                    }
                    else
                    {
                        var ccbData = cbData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
                        var totCcb = cbDetail.Where(x => x.TransCode == item.InvCode && ccbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                        item.PaidAmount = totDlv > 0 ? (totCcb * item.TransAmount / totDlv) : 0;
                        item.EndingBalance = item.TransAmount - item.PaidAmount;
                    }
                }
                else
                {
                    var totDlv = _db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == item.InvCode)?.Total ?? 0m;
                    var totCcb = cbDetail.Where(x => x.TransCode == item.InvCode && cbData.Select(y => y.Code).Contains(x.Code)).Sum(x => x.TransAmount);
                    item.PaidAmount = totDlv > 0 ? (totCcb * item.TransAmount / totDlv) : 0;
                    item.EndingBalance = item.TransAmount - item.PaidAmount;
                }
            }

            foreach (var itemBB in bbData)
            {
                var newData = new ReportByDeliveryARMutation
                {
                    Date = itemBB.Date,
                    DueDate = itemBB.DueDate,
                    Code = itemBB.Code,
                    CustCode = itemBB.CustCode,
                    CustName = itemBB.CustName,
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

                dlvData.Add(newData);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "NP")
                {
                    dlvData = dlvData.Where(x => x.EndingBalance > 0).ToList();
                }
                else if (status == "P")
                {
                    dlvData = dlvData.Where(x => x.EndingBalance == 0).ToList();
                }
            }

            dlvData = dlvData
                .Where(x => x.BeginningBalance > 0 || x.TransAmount > 0 || x.PaidAmount > 0 || x.EndingBalance > 0)
                .OrderBy(x => x.Date).ToList();

            foreach (var itemCus in cusData)
            {
                itemCus.TotalTrans = dlvData.Count(x => x.CustCode == itemCus.Code);
                itemCus.BeginningBalance = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.BeginningBalance);
                itemCus.TransAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.TransAmount);
                itemCus.PaidAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.PaidAmount);
                itemCus.EndingBalance = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.EndingBalance);
            }

            cusData = cusData.Where(x => x.TotalTrans > 0).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    dlvData = dlvData.Where(x => x.CustCode == custCode).ToList();
                }

                dlvData.Add(new ReportByDeliveryARMutation
                {
                    Code = "Total",
                    BeginningBalance = dlvData.Sum(x => x.BeginningBalance),
                    TransAmount = dlvData.Sum(x => x.TransAmount),
                    PaidAmount = dlvData.Sum(x => x.PaidAmount),
                    EndingBalance = dlvData.Sum(x => x.EndingBalance),
                });

                return dlvData.AsQueryable().ToDataSourceResult(0, dlvData.Count, null, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(custCode))
                {
                    cusData = cusData.Where(x => x.Code == custCode).ToList();
                }

                cusData.Add(new ReportByCustomerMutation
                {
                    Name = "Total",
                    BeginningBalance = cusData.Sum(x => x.BeginningBalance),
                    TransAmount = cusData.Sum(x => x.TransAmount),
                    PaidAmount = cusData.Sum(x => x.PaidAmount),
                    EndingBalance = cusData.Sum(x => x.EndingBalance),
                });

                return cusData.AsQueryable().ToDataSourceResult(0, cusData.Count, null, null);
            }
        }
    }
}
