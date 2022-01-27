using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Finance
{
    public class OutstandingChequeReportService : IOutstandingChequeReportService
    {
        private readonly TenantContext _db;
        public OutstandingChequeReportService(TenantContext db)
        {
            _db = db;
        }

		public DataSourceResult GetData(string date, string coaCode)
		{
			var ocData = _db.OutstandingChequeReports.FromSqlRaw(@"SELECT cb.[Date], cb.ChequeDate, cb.Code, cb_d.CoaName, cb.ChequeNo, cb_d.TransCode,
						CASE cb_d.[Type]
							WHEN 'AP' THEN pi_h.SupName
							WHEN 'AR' THEN si_h.CustName
							WHEN 'EPAP' THEN ep_h.SupName
							WHEN 'DPC' THEN cm.CustName
							WHEN 'RDPC' THEN cm.CustName
							WHEN 'SR' THEN cm.CustName
							WHEN 'DPS' THEN dm.SupName
							WHEN 'RPDS' THEN dm.SupName
							WHEN 'PR' THEN dm.SupName
							ELSE ''
						END AS ClientName,
						CASE 
							WHEN cb_d.TypeAmount = 'D' THEN abs(cb_d.Amount)
							ELSE CAST(0 as decimal) END AS BalanceIn,
						CASE 
							WHEN cb_d.TypeAmount = 'C' THEN abs(cb_d.Amount)
							ELSE CAST(0 as decimal) END AS BalanceOut,
						cb.CoaName AS CoaNameHeader
						FROM Finance.vwGeneralCashBankDetail cb_d
						LEFT JOIN Purchasing.vwPurchaseInvoiceHeader pi_h ON pi_h.Code = cb_d.TransCode
						LEFT JOIN Sales.vwSalesInvoiceHeader si_h ON si_h.Code = cb_d.TransCode
						LEFT JOIN Expedition.vwExpeditionInvoiceHeader ep_h ON ep_h.Code = cb_d.TransCode
						LEFT JOIN Purchasing.vwDebitMemo dm ON dm.Code = cb_d.TransCode
						LEFT JOIN Sales.vwCreditMemo cm ON cm.Code = cb_d.TransCode
						LEFT JOIN Finance.vwGeneralCashBankHeader cb ON cb.Code = cb_d.Code
						WHERE cb.ChequeDate IS NOT NULL" +
						(string.IsNullOrEmpty(coaCode) ? "" : $" AND cb_d.CoaCode = '{coaCode.Replace("'", "''")}'") +
						" ORDER BY cb.ChequeDate ASC, cb_d.CoaName ASC, BalanceIn DESC, BalanceOut DESC").ToList();

			if (!string.IsNullOrEmpty(date))
				ocData = ocData.Where(x => x.Date >= Convert.ToDateTime(date)).ToList();

			if (ocData.Any())
            {
				ocData.Add(new Entity.Finance.OutstandingChequeReport
				{
					Code = "Total",
					BalanceIn = ocData.Sum(x => x.BalanceIn),
					BalanceOut = ocData.Sum(x => x.BalanceOut)
				});
            }
			return ocData.AsQueryable().ToDataSourceResult(0, ocData.Count, null, null);
        }
    }
}
