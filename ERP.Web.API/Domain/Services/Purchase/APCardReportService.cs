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

			string whEndDate = "", whCbEndDate = "", whDmEndDate = "";
			if (!string.IsNullOrEmpty(endDate))
			{
				whEndDate = $"AND [Date] <= '{endDate.Replace("'", "''")}'";
				whCbEndDate = $"AND ISNULL(cb_h.ChequeDate, cb_h.[Date]) <= '{endDate.Replace("'", "''")}'";
				whDmEndDate = $"AND dm.[Date] <= '{endDate.Replace("'", "''")}'";
			}
			
			var cardData = _db.ReportByAPCards.FromSqlRaw(
				$@"SELECT dt.[Date], dt.Code, dt.Notes, dt.DebitAmount, dt.CreditAmount,
    				CAST(0 AS decimal(19,8)) AS RemainingAmount, dt.SupCode, CAST(0 as bit) AS IsBold
				FROM 
				(
					SELECT 
						inv.[Date], inv.Code, 'Kd. Order: ' + inv.POCode AS Notes, inv.SupCode, 
						CAST(0 AS decimal(19,8)) AS DebitAmount, inv.Total AS CreditAmount, 1 AS Sort
					FROM Purchasing.PurchaseInvoiceHeader inv
					WHERE inv.Mark <> 'V' {whEndDate}
					UNION ALL
					SELECT 
						bb.[Date], bb.Code, 'Saldo Awal Hutang' AS Notes, bb.SupCode, 
						CAST(0 AS decimal(19,8)) AS DebitAmount, bb.Amount AS CreditAmount, 1 AS Sort
					FROM Accounting.BeginningBalanceAP bb
					WHERE bb.IsActive = 1 {whEndDate}
					UNION ALL
					SELECT
						ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Faktur: ' + cb_d.TransCode AS Notes, inv.SupCode,
						cb_d.TransAmount AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 2 AS Sort
					FROM Finance.GeneralCashBankDetail cb_d
					LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
					LEFT JOIN Purchasing.PurchaseInvoiceHeader inv ON inv.Code = cb_d.TransCode
					WHERE cb_d.[Type] = 'AP' AND cb_h.Mark NOT IN ('V', 'REJ') AND cb_d.Src = 'PI' {whCbEndDate}
					UNION ALL
					SELECT
						ISNULL(cb_h.ChequeDate, cb_h.[Date]) AS [Date], cb_d.Code, 'Kd. Saldo Awal Hutang: ' + cb_d.TransCode AS Notes, bb.SupCode,
						cb_d.TransAmount AS DebitAmount, CAST(0 AS decimal(19,8)) AS CreditAmount, 2 AS Sort
					FROM Finance.GeneralCashBankDetail cb_d
					LEFT JOIN Finance.GeneralCashBankHeader cb_h ON cb_h.Code = cb_d.Code
					LEFT JOIN Accounting.BeginningBalanceAP bb ON bb.Code = cb_d.TransCode
					WHERE cb_d.[Type] = 'AP' AND cb_h.Mark NOT IN ('V', 'REJ') AND cb_d.Src = 'BB' {whCbEndDate}
					UNION ALL
					SELECT
						inv.[Date], dm.Code, 'Kd. Faktur: ' + pi_dm.InvCode AS Notes, inv.SupCode,
						pi_dm.DebitMemoAmount AS DebitAmount, CAST(0 AS decimal(19,8)) AS DebitAmount, 2 AS Sort
					FROM Purchasing.PurchaseInvoiceDebitMemo pi_dm
					LEFT JOIN Purchasing.DebitMemo dm ON dm.Code = pi_dm.DebitMemoCode
					LEFT JOIN Purchasing.PurchaseInvoiceHeader inv ON inv.Code = pi_dm.InvCode
					WHERE dm.Mark NOT IN ('V', 'PP') {whDmEndDate}
				) dt
				WHERE dt.SupCode = '{supCode.Replace("'", "''")}'
				ORDER BY dt.[Date], dt.Sort").ToList();

			if (!string.IsNullOrEmpty(startDate))
            {
				bbData = cardData.Where(x => x.Date < Convert.ToDateTime(startDate)).ToList();
				cardData = cardData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
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
