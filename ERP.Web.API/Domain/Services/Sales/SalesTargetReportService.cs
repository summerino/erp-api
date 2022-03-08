using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class SalesTargetReportService : ISalesTargetReportService
    {
        private readonly TenantContext _db;
        public SalesTargetReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(string startDate, string endDate, int? salesId, int? groupId, int? groupSubGroupId, string groupSubGroup, bool isDetail)
        {
            var detailQuery = @"SELECT inv.[Date], inv.DueDate, inv.Code, inv.CustCode, inv.CustName,
                            im.Initial AS ItemInitial, im.[Name] AS ItemName, dlv_d.Qty,
                            dlv_d.UnitId, dlv_d.UnitName, dlv_d.UnitPrice AS GrossAmount,
                            dlv_d.Disc AS Disc, dlv_d.FinalDiscHeader AS DiscHeader,
                            dlv_d.UnitPrice - dlv_d.Disc - dlv_d.FinalDiscHeader AS SubTotal,
                            dlv_d.DPP, dlv_d.TaxAmount, dlv_d.NettPrice,
                            dlv_d.UnitPrice * dlv_d.Qty AS TotalGrossAmount, (dlv_d.UnitPrice - dlv_d.Disc - dlv_d.FinalDiscHeader) * dlv_d.Qty AS TotalAfterDisc,
                            dlv_d.Disc * dlv_d.Qty AS TotalDisc, dlv_d.FinalDiscHeader * dlv_d.Qty AS TotalDiscHeader,
                            dlv_d.DPP * dlv_d.Qty AS TotalDPP, dlv_d.TaxAmount * dlv_d.Qty AS TotalTaxAmount, dlv_d.Total, dlv_d.NettPrice * dlv_d.Qty AS TotalNettPrice,
                            CASE inv.Mark
	                            WHEN 'A' THEN 'Aktif'
	                            WHEN 'PP' THEN 'Aktif'
	                            WHEN 'CMP' THEN 'Aktif' END AS [Status],
                            ic.Id AS CategoryId, ic.Initial AS CategoryInitial,
                            inv.SOCode AS OrderCode, dlv.TaxInvoiceDate, dlv.TaxInvoiceNo, dlv.Code AS DoCode,
                            ig.Id AS ItemGroupId, igs.Id AS ItemSubGroupId, im.SubGroup1, im.SubGroup2, im.SubGroup3,
                            im.SubGroup4, im.SubGroup5, so.SalesBy AS SalesId
                            FROM Sales.SalesInvoiceDetail inv_d
                            LEFT JOIN Sales.vwSalesInvoiceHeader inv ON inv.Code = inv_d.Code
                            LEFT JOIN Sales.SalesOrderHeader so ON so.Code = inv.SOCode
                            LEFT JOIN Sales.vwSalesDeliveryHeader dlv ON inv_d.DOCode = dlv.Code  
                            LEFT JOIN Sales.vwSalesDeliveryDetail dlv_d on dlv_d.Code = dlv.Code
                            LEFT JOIN Inventory.Item im ON im.Id = dlv_d.ItemId
                            LEFT JOIN Inventory.ItemCategory ic ON ic.Id = im.CategoryId
                            LEFT JOIN Inventory.ItemGroup ig ON ig.Id = ic.GroupId
                            LEFT JOIN Inventory.ItemGroupSubGroup igs ON igs.ItemGroupId = ig.Id
                            WHERE inv.Mark != 'V'" +
                            (!salesId.HasValue || salesId <= 0 ? "" : $" AND so.SalesBy = {salesId}") +
                            (!groupId.HasValue || groupId <= 0 ? "" : $" AND ig.Id = {groupId}") +
                            (!groupSubGroupId.HasValue || groupSubGroupId <= 0 ? "" : $" AND igs.Id = {groupSubGroupId}") +
                            (string.IsNullOrEmpty(groupSubGroup) ? "" : @$" AND (im.SubGroup1 = '{groupSubGroup}'
                            OR im.SubGroup2 = '{groupSubGroup}' OR im.SubGroup3 = '{groupSubGroup}' OR im.SubGroup4 = '{groupSubGroup}'
                            OR im.SubGroup5 = '{groupSubGroup}')" +
                            (string.IsNullOrEmpty(startDate) ? "" : $" AND inv.Date >= '{startDate}'") +
                            (string.IsNullOrEmpty(endDate) ? "" : $" AND inv.Date <= '{endDate}'"));

            var tsDetailData = _db.ReportByDetailSTs.FromSqlRaw(detailQuery).ToList();
            if (isDetail)
            {
                tsDetailData.Add(new Entity.Sales.ReportByDetailST
                {
                    Code = "Total",
                    Qty = tsDetailData.Sum(x => x.Qty),
                    TotalGrossAmount = tsDetailData.Sum(x => x.TotalGrossAmount),
                    TotalDisc = tsDetailData.Sum(x => x.TotalDisc),
                    TotalDiscHeader = tsDetailData.Sum(x => x.TotalDiscHeader),
                    TotalAfterDisc = tsDetailData.Sum(x => x.TotalAfterDisc),
                    TotalDpp = tsDetailData.Sum(x => x.TotalDpp),
                    TotalTaxAmount = tsDetailData.Sum(x => x.TotalTaxAmount),
                    TotalNettPrice = tsDetailData.Sum(x => x.TotalNettPrice)
                });

                return tsDetailData.AsQueryable().ToDataSourceResult(0, tsDetailData.Count, null, null);
            }
            else
            {
                var tsData = _db.ReportBySTs.FromSqlRaw(@"SELECT st_s.SalesmanId AS SalesId, emp.FirstName AS SalesName,
                    st_d.ItemGroupId, ig.[Name] AS ItemGroup,
                    st_d.ItemSubGroupId, st_d.SubGroup AS ItemSubGroup2,
                    igs.[Name] + ' - ' + st_d.SubGroup AS ItemSubGroup, 
                    SUM(st_d.Amount) AS TargetAmount,
                    CAST(0 AS int) AS TotalCustomers,
                    CAST(0 AS decimal) AS RealAmount,
                    CAST(0 AS decimal) AS TargetPercent
                    FROM Sales.SalesTargetHeader st_h
                    LEFT JOIN Sales.SalesTargetDetail st_d ON st_d.Code = st_h.Code
                    LEFT JOIN Sales.SalesTargetSubject st_s ON st_s.Code = st_h.Code
                    LEFT JOIN General.Employee emp ON emp.Id = st_s.SalesmanId
                    LEFT JOIN Inventory.ItemGroup ig ON ig.Id = st_d.ItemGroupId
                    LEFT JOIN Inventory.ItemGroupSubGroup igs ON igs.Id = st_d.ItemSubGroupId
                    WHERE st_h.Mark != 'V'" +
                    (string.IsNullOrEmpty(startDate) ? "" : $" AND st_h.StartDate >= '{startDate}'") +
                    (string.IsNullOrEmpty(endDate) ? "" : $" AND st_h.EndDate <= '{endDate}'") +
                    @" GROUP BY st_s.SalesmanId, emp.FirstName, st_d.ItemGroupId, ig.[Name],
                    st_d.ItemSubGroupId, igs.[Name], st_d.SubGroup").ToList();

                foreach (var item in tsData)
                {
                    var itemDetailData = tsDetailData
                        .Where(x => x.SalesId == item.SalesId &&
                        x.ItemGroupId == item.ItemGroupId &&
                        x.ItemSubGroupId == item.ItemSubGroupId &&
                        (x.SubGroup1 == item.ItemSubGroup2 || x.SubGroup2 == item.ItemSubGroup2 ||
                        x.SubGroup3 == item.ItemSubGroup2 || x.SubGroup4 == item.ItemSubGroup2 ||
                        x.SubGroup5 == item.ItemSubGroup2) &&
                        x.Date >= Convert.ToDateTime(startDate) &&
                        x.Date <= Convert.ToDateTime(endDate)).ToList();

                    item.TotalCustomers = itemDetailData.DistinctBy(x => x.CustCode).Count();
                    item.RealAmount = itemDetailData.Sum(x => x.TotalNettPrice);
                    item.TargetPercent = item.RealAmount > 0 ? item.RealAmount / item.TargetAmount * 100 : 0m;
                }

                if (salesId.HasValue || salesId > 0)
                {
                    tsData = tsData.Where(x => x.SalesId == salesId).ToList();
                }

                tsData.Add(new Entity.Sales.ReportByST
                {
                    ItemGroup = "Total",
                    TargetAmount = tsData.Sum(x => x.TargetAmount),
                    RealAmount = tsData.Sum(x => x.RealAmount),
                    TargetPercent = tsData.Any() ? tsData.Sum(x => x.RealAmount) / tsData.Sum(x => x.TargetAmount) * 100 : 0
                });

                return tsData.AsQueryable().ToDataSourceResult(0, tsData.Count, null, null);
            }
        }
    }
}
