using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class ARCardReportService : IARCardReportService
    {
        private readonly TenantContext _db;
        public ARCardReportService(TenantContext db)
        {
            _db = db;
        }

		public DataSourceResult GetData(string startDate, string endDate, string custCode, int? salesId)
		{
			List<ReportByARCard> cardListData = new();
			List<ReportByARCard> bbData = new();
			var cardData = _db.ReportByARCards.FromSqlRaw(
							$@"SELECT dt.[Date], dt.Code, dt.Notes, dt.DebitAmount, dt.CreditAmount, CAST(0 AS decimal(19,8)) AS RemainingAmount, dt.CustCode, dt.SalesId, CAST(0 as bit) AS IsBold
							FROM 
							(
								SELECT 
									inv.[Date], inv.Code, 'Kd. Order: ' + inv.SOCode AS Notes, inv.CustCode, so.SalesBy AS SalesId,
									inv.Total AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 1 AS Sort
								FROM Sales.SalesInvoiceHeader inv
								LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
								WHERE inv.Mark NOT IN ('V', 'OL')
								UNION
								SELECT 
									ar.[Date], ar.Code, 'Saldo Awal Hutang' AS Notes, ar.CustCode, CAST(0 as bigint) AS SalesId, 
									ar.Amount AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 1 AS Sort
								FROM Accounting.BeginningBalanceAR ar
								UNION
								SELECT
									ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Faktur: ' + cb_d.TransCode AS Notes, inv.CustCode, so.SalesBy AS SalesId,
									CAST(0 AS decimal(19,8)) AS DebitAmount, cb_d.TransAmount AS CreditAmount, 2 AS Sort
								FROM Finance.GeneralCashBankDetail cb_d
								LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
								LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = cb_d.TransCode
								LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
								WHERE cb_d.[Type] = 'AR' AND cb_h.Mark <> 'V' AND cb_d.Src = 'SI'
								UNION
								SELECT
									ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Saldo Awal Hutang: ' + cb_d.TransCode AS Notes, ar.CustCode, CAST(0 as bigint) AS SalesId, 
									CAST(0 AS decimal(19,8)) AS DebitAmount, cb_d.TransAmount AS CreditAmount, 2 AS Sort
								FROM Finance.GeneralCashBankDetail cb_d
								LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
								LEFT JOIN Accounting.BeginningBalanceAR ar ON ar.Code = cb_d.TransCode
								WHERE cb_d.[Type] = 'AR' AND cb_h.Mark <> 'V' AND cb_d.Src = 'BB'
								UNION
								SELECT
									cm.[Date], cm.Code, 'Kd. Faktur: ' + si_cm.InvCode AS Notes, inv.CustCode, so.SalesBy AS SalesId,
									CAST(0 AS decimal(19,8)) AS DebitAmount, si_cm.CreditMemoAmount AS CreditAmount, 2 AS Sort
								FROM Sales.SalesInvoiceCreditMemo si_cm
								LEFT JOIN Sales.CreditMemo cm ON cm.Code = si_cm.CreditMemoCode
								LEFT JOIN Sales.SalesInvoiceHeader inv ON inv.Code = si_cm.InvCode
								LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
								WHERE cm.Mark NOT IN ('V', 'PP')
							) dt
							WHERE dt.CustCode = '{custCode.Replace("'", "''")}'" +
							(salesId.HasValue ? $" AND dt.SalesId = {salesId.Value}" : "") +
							" ORDER BY dt.[Date], dt.Sort")
							.ToList();

			if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
			{
				bbData = cardData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
				cardData = cardData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
			}
			else if (!string.IsNullOrEmpty(startDate))
			{
				bbData = cardData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
				cardData = cardData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
			}
			else if (!string.IsNullOrEmpty(endDate))
			{
				cardData = cardData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
			}

			cardListData.Add(new ReportByARCard
			{
				Code = "Saldo Awal",
				RemainingAmount = bbData.Count > 0 ? bbData.Sum(x => x.DebitAmount - x.CreditAmount) : 0m,
				IsBold = true
			});

			var remainAmount = bbData.Count > 0 ? bbData.Sum(x => x.DebitAmount - x.CreditAmount) : 0m;
			foreach (var item in cardData)
			{
				remainAmount += item.DebitAmount - item.CreditAmount;
				item.RemainingAmount = remainAmount;

				cardListData.Add(item);
			}

			cardListData.Add(new ReportByARCard
			{
				Code = "Total",
				DebitAmount = cardListData.Where(x => !x.IsBold).Sum(x => x.DebitAmount),
				CreditAmount = cardListData.Where(x => !x.IsBold).Sum(x => x.CreditAmount),
				RemainingAmount = remainAmount,
				IsBold = true
			});

			return cardListData.AsQueryable().ToDataSourceResult(0, cardListData.Count, null, null);
		}
    }
}
