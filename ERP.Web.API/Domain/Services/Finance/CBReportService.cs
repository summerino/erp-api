using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Web.API.Domain.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Finance;

public class CBReportService : ICBReportService
{
    private readonly TenantContext _db;

    public CBReportService(TenantContext db)
    {
        _db = db;
    }

    public DataSourceResult GetData(int? type, string startDate, string endDate, string coaCode, IEnumerable<Sort> sorts)
    {
        List<ReportByAllAccount> reportAC = new();
        List<ReportByAccount> reportA = new();
        List<ReportByAccountDetail> reportAD = new();

        var dataCOA = _db.Coas.Where(x => x.IsActive).ToList();
        var dataPCOA = dataCOA;

        dataCOA = dataCOA.Where(x => !dataPCOA.Select(t => t.ParentId).Contains(x.Id)).ToList();
        dataCOA = dataCOA.Where(x => x.TypeId == 1).ToList();

        var dataCBHeader = _db.GeneralCashBankHeaders.Where(x => x.Mark == "A").ToList();
        var dataCBDetail = _db.GeneralCashBankDetails.ToList();

        var initCBHeader = dataCBHeader.Where(x => x.ChequeDate.GetValueOrDefault(x.Date) < Convert.ToDateTime(startDate)).ToList();

        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            dataCBHeader = dataCBHeader.Where(x => x.ChequeDate.GetValueOrDefault(x.Date) >= Convert.ToDateTime(startDate) && x.ChequeDate.GetValueOrDefault(x.Date) <= Convert.ToDateTime(endDate)).ToList();
        }
        else if (!string.IsNullOrEmpty(endDate))
        {
            dataCBHeader = dataCBHeader.Where(x => x.ChequeDate.GetValueOrDefault(x.Date) <= Convert.ToDateTime(endDate)).ToList();
        }

        if (!string.IsNullOrEmpty(coaCode))
        {
            dataCOA = dataCOA.Where(x => x.Code == coaCode).ToList();
            dataCBHeader = dataCBHeader.Where(x => x.CoaCode == coaCode).ToList();
            dataCBDetail = dataCBDetail.Where(x => dataCBHeader.Select(h => h.Code).Contains(x.Code)).ToList();
            initCBHeader = initCBHeader.Where(x => x.CoaCode == coaCode).ToList();
        }


        if (type == 1)
        {
            var endBalance = initCBHeader.Sum(x => x.Amount);
            reportA.Add(new ReportByAccount
            {
                Code = "Saldo Awal",
                EndingBalance = endBalance,
                IsBold = true
            });

            foreach (var item in dataCBHeader.OrderBy(x => x.ChequeDate.GetValueOrDefault(x.Date)))
            {
                endBalance += item.Amount;

                reportA.Add(new ReportByAccount
                {
                    Date = item.ChequeDate.GetValueOrDefault(item.Date),
                    Code = item.Code,
                    Notes = item.Notes,
                    IncomingBalance = item.Type == "D" ? item.Amount : 0,
                    OutgoingBalance = item.Type == "D" ? 0 : Math.Abs(item.Amount),
                    EndingBalance = endBalance,
                    IsBold = false
                });
            }

            reportA.Add(new ReportByAccount
            {
                Code = "Saldo Akhir",
                IncomingBalance = reportA.Where(x => !x.IsBold).Sum(x => x.IncomingBalance),
                OutgoingBalance = reportA.Where(x => !x.IsBold).Sum(x => x.OutgoingBalance),
                EndingBalance = endBalance,
                IsBold = true
            });

            return reportA.AsQueryable().ToDataSourceResult(0, reportA.Count, null, null);
        }
        else if (type == 2)
        {
            var endBalance = initCBHeader.Sum(x => x.Amount);
            reportAD.Add(new ReportByAccountDetail
            {
                Code = "Saldo Awal",
                EndingBalance = endBalance,
                IsBold = true
            });

            foreach (var item in dataCBHeader.OrderBy(x => x.ChequeDate.GetValueOrDefault(x.Date)))
            {
                var selectedDetail = dataCBDetail.Where(x => x.Code == item.Code).ToList();
                foreach (var itemDetail in selectedDetail)
                {
                    if(itemDetail.TypeAmount == "C")
                    {
                        endBalance += itemDetail.Amount;
                    }
                    else
                    {
                        endBalance -= itemDetail.Amount;
                    }

                    var sysCoa = _db.SystemParameters.FirstOrDefault(x => x.Value == itemDetail.CoaCode);
                    var coa = _db.Coas.FirstOrDefault(x => x.Code == itemDetail.CoaCode);

                    reportAD.Add(new ReportByAccountDetail
                    {
                        Date = item.ChequeDate.GetValueOrDefault(item.Date),
                        Code = item.Code,
                        Notes = itemDetail.Notes,
                        TransCode = itemDetail.TransCode,
                        CoaCode = itemDetail.CoaCode,
                        CoaName = sysCoa?.Description ?? coa?.Name ?? "",
                        IncomingBalance = itemDetail.TypeAmount == "C" ? itemDetail.Amount : 0,
                        OutgoingBalance = itemDetail.TypeAmount == "D" ? itemDetail.Amount : 0,
                        EndingBalance = endBalance,
                        IsBold = false
                    });
                }
            }

            reportAD.Add(new ReportByAccountDetail
            {
                Code = "Saldo Akhir",
                IncomingBalance = reportAD.Where(x => !x.IsBold).Sum(x => x.IncomingBalance),
                OutgoingBalance = reportAD.Where(x => !x.IsBold).Sum(x => x.OutgoingBalance),
                EndingBalance = endBalance,
                IsBold = true
            });

            return reportAD.AsQueryable().ToDataSourceResult(0, reportAD.Count, null, null);
        }
        else
        {
            foreach (var item in dataCOA)
            {
                var bBalance = initCBHeader.Where(x => x.CoaCode == item.Code).Sum(x => x.Amount);
                var iBalance = dataCBHeader.Where(x => x.CoaCode == item.Code && x.Type == "D").Sum(x => x.Amount);
                var oBalance = Math.Abs(dataCBHeader.Where(x => x.CoaCode == item.Code && x.Type == "C").Sum(x => x.Amount));
                var eBalance = (bBalance + iBalance) - oBalance;
                reportAC.Add(new ReportByAllAccount
                {
                    Code = item.Code,
                    Name = item.Name,
                    BeginningBalance = bBalance,
                    IncomingBalance = iBalance,
                    OutgoingBalance = oBalance,
                    EndingBalance = eBalance
                });
            }
            return reportAC.AsQueryable().ToDataSourceResult(0, reportAC.Count, null, sorts);
        }
    }
}