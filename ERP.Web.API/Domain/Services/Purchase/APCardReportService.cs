using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Purchase
{
    public class APCardReportService : IAPCardReportService
    {
        private readonly TenantContext _db;
        public APCardReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(string startDate, string endDate, string supCode)
        {
			List<ReportByAPCard> cardListData = new();
			List<ReportByAPCard> bbData = new();
			var cardData = _db.ReportByAPCards.FromSqlRaw(
							$@"SELECT dt.[Date], dt.Code, dt.Notes, dt.DebitAmount, dt.CreditAmount, CAST(0 AS decimal(19,8)) AS RemainingAmount, dt.SupCode, CAST(0 as bit) AS IsBold
							FROM 
							(
								SELECT 
									inv.[Date], inv.Code, 'Kd. Order: ' + inv.POCode AS Notes, inv.SupCode, 
									CAST(0 AS decimal(19,8)) AS DebitAmount, inv.Total AS CreditAmount, 1 AS Sort
								FROM Purchasing.PurchaseInvoiceHeader inv
								UNION
								SELECT 
									ap.[Date], ap.Code, 'Saldo Awal Hutang' AS Notes, ap.SupCode, 
									CAST(0 AS decimal(19,8)) AS DebitAmount, ap.Amount AS CreditAmount, 1 AS Sort
								FROM Accounting.BeginningBalanceAP ap
								UNION
								SELECT
									ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Faktur: ' + cb_d.TransCode AS Notes, inv.SupCode,
									cb_d.TransAmount AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 2 AS Sort
								FROM Finance.GeneralCashBankDetail cb_d
								LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
								LEFT JOIN Purchasing.PurchaseInvoiceHeader inv ON inv.Code = cb_d.TransCode
								WHERE cb_d.[Type] = 'AP' AND cb_h.Mark <> 'V' AND cb_d.Src = 'PI'
								UNION
								SELECT
									ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Saldo Awal Hutang: ' + cb_d.TransCode AS Notes, ap.SupCode,
									cb_d.TransAmount AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 2 AS Sort
								FROM Finance.GeneralCashBankDetail cb_d
								LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
								LEFT JOIN Accounting.BeginningBalanceAP ap ON ap.Code = cb_d.TransCode
								WHERE cb_d.[Type] = 'AP' AND cb_h.Mark <> 'V' AND cb_d.Src = 'BB'
							) dt
							WHERE dt.SupCode = '{supCode.Replace("'", "''")}'
							ORDER BY dt.[Date], dt.Sort")
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

			cardListData.Add(new ReportByAPCard
			{
				Code = "Saldo Awal",
				RemainingAmount = bbData.Count > 0 ? bbData.Sum(x => x.CreditAmount - x.DebitAmount) : 0m,
				IsBold = true
			});

			var remainAmount = bbData.Count > 0 ? bbData.Sum(x => x.CreditAmount - x.DebitAmount) : 0m;
			foreach (var item in cardData)
            {
				remainAmount += item.CreditAmount - item.DebitAmount;
				item.RemainingAmount = remainAmount;

				cardListData.Add(item);
            }

			cardListData.Add(new ReportByAPCard
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
