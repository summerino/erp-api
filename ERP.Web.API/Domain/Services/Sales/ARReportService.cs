using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class ARReportService : IARReportService
{
    private readonly TenantContext _db;
    public ARReportService(TenantContext db)
    {
        _db = db;
    }
    public DataSourceResult GetData(int type, string date, string custCode, int slsId, IEnumerable<Sort> sorts)
    {
        var sysData = _db.SystemParameters.FirstOrDefault(x => x.Code == "AR_RECOG_TIME");
        if (sysData != null)
        {
            if (sysData.Value == "SI")
            {
                var cusData = _db.ReportByCustomers.FromSqlRaw(@"SELECT a.Code, a.Initial, a.[Name], SUM(a.TotalTrans) AS TotalTrans, SUM(a.TotalAmount) AS TotalAmount,
                            CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount 
                            FROM (
                            select cs.Code, cs.Initial, cs.Name, count(*) as TotalTrans, sum(inv.Total) as TotalAmount
                                                        from General.Customer cs
                                                        left join Sales.SalesInvoiceHeader inv on inv.CustCode = cs.Code
                                                        Where inv.Mark IN('A', 'PP', 'CMP') Group by cs.Code, cs.Initial, cs.Name
							                            UNION
                            select cs.Code, cs.Initial, cs.Name, count(*) as TotalTrans, sum(ar.Amount) as TotalAmount
                                                        from General.Customer cs
                                                        left join Accounting.BeginningBalanceAR ar on ar.CustCode = cs.Code
                                                        Where ar.IsActive = 1 Group by cs.Code, cs.Initial, cs.Name) a
                            Group by a.Code, a.Initial, a.[Name]").ToList();

                var invData = _db.ReportByInvoiceARs.FromSqlRaw(@"SELECT inv.Date, inv.DueDate, inv.Code, inv.SOCode AS OrderCode, 
                            e.Id AS SalesId, e.FirstName AS SalesName,  
                            inv.CustCode, cust.[Name] AS CustName, 
                            inv.Total AS TotalAmount, CAST (0 AS decimal) AS PaidAmount, CAST (0 AS decimal) AS RemainderAmount
                            FROM Sales.SalesInvoiceHeader inv
                            LEFT JOIN General.Customer cust on cust.Code = inv.CustCode
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
                            LEFT JOIN General.Employee e ON e.Id = so.SalesBy  
                            WHERE inv.Mark IN('A', 'PP', 'CMP')" + (slsId > 0 ? $" and so.SalesBy = {slsId} " : " ") + "").ToList();

                var cbData = _db.GeneralCashBankHeaders.Where(x => !new[] { "V", "REJ" }.Contains(x.Mark) && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

                var bbData = _db.VwBeginningBalanceARs.Where(x => x.IsActive && x.Date <= Convert.ToDateTime(date)).ToList();

                var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

                invData = invData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

                var invCMData = _db.SalesInvoiceCreditMemos.Where(x => invData.Select(y => y.Code).Contains(x.InvCode)).ToList();

                var cmData = _db.CreditMemos.Where(x => x.Mark != "V" && x.Date <= Convert.ToDateTime(date)).ToList();

                foreach (var itemInv in invData)
                {
                    var totCb = cbDetail.Where(x => x.TransCode == itemInv.Code).Sum(x => x.TransAmount);
                    var totCm = invCMData.Where(x => x.InvCode == itemInv.Code && cmData.Select(y => y.Code).Contains(x.CreditMemoCode))
                                    .Sum(x => x.CreditMemoAmount);
                    itemInv.PaidAmount = totCb + totCm;
                    itemInv.RemainderAmount = itemInv.TotalAmount - itemInv.PaidAmount;
                }

                if (slsId <= 0)
                {
                    foreach (var itemBB in bbData)
                    {
                        var totCb = cbDetail.Where(x => x.TransCode == itemBB.Code).Sum(x => x.TransAmount);
                        invData.Add(new Entity.Sales.ReportByInvoiceAR
                        {
                            Date = itemBB.Date,
                            DueDate = itemBB.DueDate,
                            Code = itemBB.Code,
                            OrderCode = "",
                            CustCode = itemBB.CustCode,
                            CustName = itemBB.CustName,
                            TotalAmount = itemBB.Amount,
                            PaidAmount = totCb,
                            RemainderAmount = itemBB.Amount - totCb
                        });
                    }
                }
                
                invData = invData.Where(x => x.RemainderAmount > 0).ToList();

                foreach (var itemCus in cusData)
                {
                    itemCus.TotalTrans = invData.Count(x => x.CustCode == itemCus.Code);
                    itemCus.TotalAmount = invData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.TotalAmount);
                    itemCus.PaidAmount = invData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.PaidAmount);
                    itemCus.RemainderAmount = invData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.RemainderAmount);
                }

                cusData = cusData.Where(x => x.TotalTrans > 0).ToList();

                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(custCode))
                    {
                        invData = invData.Where(x => x.CustCode == custCode).ToList();
                    }
                    return invData.AsQueryable().ToDataSourceResult(0, invData.Count, null, sorts);
                }
                else
                {
                    if (!string.IsNullOrEmpty(custCode))
                    {
                        cusData = cusData.Where(x => x.Code == custCode).ToList();
                    }
                    return cusData.AsQueryable().ToDataSourceResult(0, cusData.Count, null, sorts);
                }
            }
            else
            {
                var cusData = _db.ReportByCustomers.FromSqlRaw(@"select cs.Code, cs.Initial, cs.Name, count(*) as TotalTrans, sum(dlv.Total) as TotalAmount,
                            CAST (0 as decimal) as PaidAmount, CAST (0 as decimal) as RemainderAmount
                            from General.Customer cs
                            left join Sales.SalesDeliveryHeader dlv on dlv.CustCode = cs.Code
                            left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code
                            left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A', 'PP', 'CMP')
                            Where dlv.Mark IN('A', 'INV') Group by cs.Code, cs.Initial, cs.Name").ToList();

                var dlvData = _db.ReportByDeliveries.FromSqlRaw(@"select dlv.Date, inv.DueDate, dlv.Code, dlv.TransCode as SrcCode, inv.Code as InvCode,
                            sls.Initial as SlsInitial, sls.FirstName as SlsName, dlv.CustCode, sp.[Name] as CustName, dlv.Total as TotalAmount,
                            cast(0 as decimal) as PaidAmount, cast(0 as decimal) as RemainderAmount
                            from Sales.SalesDeliveryHeader dlv
                            left join Sales.SalesOrderHeader so on so.Code = dlv.TransCode
                            left join General.Employee sls on sls.Id = so.SalesBy
                            left join General.Customer sp on sp.Code = dlv.CustCode
                            left join Sales.SalesInvoiceDetail invD on invD.DOCode = dlv.Code
                            left join Sales.SalesInvoiceHeader inv on inv.Code = invD.Code and inv.Mark IN('A','PP','CMP')
                            Where dlv.Mark IN('A','INV')" + (slsId > 0 ? $" and so.SalesBy = {slsId} " : " ") + "").ToList();

                var cbData = _db.GeneralCashBankHeaders.Where(x => x.Mark != "V" && (x.ChequeDate ?? x.Date) <= Convert.ToDateTime(date)).ToList();

                var bbData = _db.VwBeginningBalanceARs.Where(x => x.IsActive && x.Date <= Convert.ToDateTime(date)).ToList();

                var cbDetail = _db.GeneralCashBankDetails.Where(x => cbData.Select(c => c.Code).Contains(x.Code)).ToList();

                dlvData = dlvData.Where(x => x.Date <= Convert.ToDateTime(date)).ToList();

                var invCMData = _db.SalesInvoiceCreditMemos.Where(x => dlvData.Select(y => y.InvCode).Contains(x.InvCode)).ToList();

                var cmData = _db.CreditMemos.Where(x => x.Mark != "V" && x.Date <= Convert.ToDateTime(date)).ToList();

                foreach (var itemDlv in dlvData)
                {
                    var invData = _db.SalesInvoiceCreditMemos.Where(x => x.InvCode == itemDlv.InvCode).ToList();
                    var totDlv = dlvData.Where(x => x.InvCode == itemDlv.InvCode).Sum(x => x.TotalAmount);
                    var totCb = cbDetail.Where(x => x.TransCode == itemDlv.InvCode).Sum(x => x.TransAmount);
                    var totCm = invCMData.Where(x => x.InvCode == itemDlv.InvCode && cmData.Select(y => y.Code).Contains(x.CreditMemoCode))
                                    .Sum(x => x.CreditMemoAmount);
                    itemDlv.PaidAmount = totDlv > 0 ? (totCb * itemDlv.TotalAmount / totDlv) + (totCm * itemDlv.TotalAmount / totDlv) : 0 + (totCm * itemDlv.TotalAmount / totDlv);
                    itemDlv.RemainderAmount = itemDlv.TotalAmount - itemDlv.PaidAmount;
                }

                foreach (var itemBB in bbData)
                {
                    var totCb = cbDetail.Where(x => x.TransCode == itemBB.Code).Sum(x => x.TransAmount);
                    dlvData.Add(new Entity.Sales.ReportByDelivery
                    {
                        Date = itemBB.Date,
                        DueDate = itemBB.DueDate,
                        Code = itemBB.Code,
                        SrcCode = "",
                        InvCode = "",
                        SlsInitial = "",
                        SlsName = "",
                        CustCode = itemBB.CustCode,
                        CustName = itemBB.CustName,
                        TotalAmount = itemBB.Amount,
                        PaidAmount = totCb,
                        RemainderAmount = itemBB.Amount - totCb
                    });
                }

                dlvData = dlvData.Where(x => x.RemainderAmount > 0).ToList();

                foreach (var itemCus in cusData)
                {
                    itemCus.TotalTrans = dlvData.Count(x => x.CustCode == itemCus.Code);
                    itemCus.TotalAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.TotalAmount);
                    itemCus.PaidAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.PaidAmount);
                    itemCus.RemainderAmount = dlvData.Where(x => x.CustCode == itemCus.Code).Sum(x => x.RemainderAmount);
                }

                cusData = cusData.Where(x => x.TotalTrans > 0).ToList();

                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(custCode))
                    {
                        dlvData = dlvData.Where(x => x.CustCode == custCode).ToList();
                    }
                    return dlvData.AsQueryable().ToDataSourceResult(0, dlvData.Count, null, sorts);
                }
                else
                {
                    if (!string.IsNullOrEmpty(custCode))
                    {
                        cusData = cusData.Where(x => x.Code == custCode).ToList();
                    }
                    return cusData.AsQueryable().ToDataSourceResult(0, cusData.Count, null, sorts);
                }
            }
        }
        else
        {
            return new DataSourceResult();
        }
    }
}