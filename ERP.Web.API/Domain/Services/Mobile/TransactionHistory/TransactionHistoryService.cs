using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

namespace ERP.Web.API.Domain.Services.Mobile.TransactionHistory
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        protected TenantContext Db;

        public TransactionHistoryService(TenantContext db)
        {
            Db = db;
        }

        public DataSourceResult GetDataByCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {

            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join cust in Db.Customers on so.CustCode equals cust.Code
                              group new { so, cust } by new { so.CustCode, cust.Name, so.SalesBy, so.Date } into g
                              select new TransactionHistoryByCustomer
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  CustomerId = g.Key.CustCode,
                                  CustomerName = g.Key.Name,
                                  Total = g.Sum(tl => tl.so.Total)
                              }).AsQueryable();

            var dataOrder = (
                        from so in Db.SalesOrderHeaders
                        join cust in Db.Customers on so.CustCode equals cust.Code
                        group new { so, cust } by new { so.CustCode, cust.Name, so.SalesBy, so.Date } into g
                        select new TransactionHistoryByCustomer
                        {
                            SalesId = g.Key.SalesBy,
                            Date = g.Key.Date,
                            CustomerId = g.Key.CustCode,
                            CustomerName = g.Key.Name,
                            Total = g.Sum(tl => tl.so.Total)
                        }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.CustomerId, so.CustomerName, so.SalesId, so.Date } into g
                        select new TransactionHistoryByCustomer
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            CustomerId = g.Key.CustomerId,
                            CustomerName = g.Key.CustomerName,
                            Total = g.Sum(tl => tl.Total)
                        }).OrderBy(x => x.Date).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                             x.CustomerName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }


        public DataSourceResult GetItemDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join c in Db.Customers on so.CustCode equals c.Code
                              join i in Db.Items on sod.ItemId equals i.Id
                              join u in Db.UoMs on sod.UomId equals u.Id
                              group new { so, sod, i, u } by new { so.Date, so.SalesBy, c.Code, sod.ItemId, i.Name, sod.UnitPrice, sod.UomId, u.BaseUnit } into g
                              select new TransactionItemDetail
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  CustomerId = g.Key.Code,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.BaseUnit,
                                  Price = g.Key.UnitPrice,
                                  Discount = g.Sum(dc => dc.sod.Disc),
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join c in Db.Customers on so.CustCode equals c.Code
                             join i in Db.Items on sod.ItemId equals i.Id
                             join u in Db.UoMs on sod.UomId equals u.Id
                             group new { so, sod, i, u } by new { so.Date, so.SalesBy, c.Code, sod.ItemId, i.Name, sod.UnitPrice, sod.UomId, u.BaseUnit } into g
                             select new TransactionItemDetail
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 CustomerId = g.Key.Code,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.BaseUnit,
                                 Price = g.Key.UnitPrice,
                                 Discount = g.Sum(dc => dc.sod.Disc),
                                 Total = g.Sum(tl => tl.sod.Total)
                             }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.Date, so.SalesId, so.CustomerId, so.ItemId, so.ItemName, so.Price, so.Unit } into g
                        select new TransactionItemDetail
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            CustomerId = g.Key.CustomerId,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Quantity = g.Sum(qt => qt.Quantity),
                            Unit = g.Key.Unit,
                            Price = g.Key.Price,
                            Discount = g.Sum(dc => dc.Discount),
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                             x.ItemName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetCustomerDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join c in Db.Customers on so.CustCode equals c.Code
                              join u in Db.UoMs on sod.UomId equals u.Id
                              group new { so, sod } by new { so.Date, so.SalesBy, c.Code, c.Name, sod.ItemId, sod.UnitPrice, sod.UomId, u.BaseUnit } into g
                              select new TransactionCustomerDetail
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  CustomerId = g.Key.Code,
                                  CustomerName = g.Key.Name,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.BaseUnit,
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join c in Db.Customers on so.CustCode equals c.Code
                             join u in Db.UoMs on sod.UomId equals u.Id
                             group new { so, sod } by new { so.Date, so.SalesBy, c.Code, c.Name, sod.ItemId, sod.UnitPrice, sod.UomId, u.BaseUnit } into g
                             select new TransactionCustomerDetail
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 CustomerId = g.Key.Code,
                                 CustomerName = g.Key.Name,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.BaseUnit,
                                 Total = g.Sum(tl => tl.sod.Total)
                             }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.Date, so.SalesId, so.CustomerId, so.CustomerName, so.ItemId, so.ItemName, so.Unit } into g
                        select new TransactionCustomerDetail
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            CustomerId = g.Key.CustomerId,
                            CustomerName = g.Key.CustomerName,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Quantity = g.Sum(qt => qt.Quantity),
                            Unit = g.Key.Unit,
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                             x.ItemName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataByDate(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort)
        {
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              group so by new { so.SalesBy, so.Date } into g
                              select new TransactionHistoryByDate
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  Total = g.Sum(tl => tl.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             group so by new { so.SalesBy, so.Date } into g
                             select new TransactionHistoryByDate
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 Total = g.Sum(tl => tl.Total)
                             }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.SalesId, so.Date } into g
                        select new TransactionHistoryByDate
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataByProduct(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                        join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                        join i in Db.Items on sod.ItemId equals i.Id
                        join u in Db.UoMs on sod.UomId equals u.Id
                        group new { so, sod, i, u } by new { so.SalesBy, so.Date, sod.ItemId, i.Name, sod.UomId, u.BaseUnit } into g
                        select new TransactionHistoryByProduct
                        {
                            SalesId = g.Key.SalesBy,
                            Date = g.Key.Date,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.Name,
                            Quantity = g.Sum(qt => qt.sod.Qty),
                            Unit = g.Key.BaseUnit,
                            Total = g.Sum(tl => tl.sod.Total)
                        }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                        join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                        join i in Db.Items on sod.ItemId equals i.Id
                        join u in Db.UoMs on sod.UomId equals u.Id
                        group new { so, sod, i, u } by new { so.SalesBy, so.Date, sod.ItemId, i.Name, sod.UomId, u.BaseUnit } into g
                        select new TransactionHistoryByProduct
                        {
                            SalesId = g.Key.SalesBy,
                            Date = g.Key.Date,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.Name,
                            Quantity = g.Sum(qt => qt.sod.Qty),
                            Unit = g.Key.BaseUnit,
                            Total = g.Sum(tl => tl.sod.Total)
                        }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.SalesId, so.Date, so.ItemId, so.ItemName, so.Unit } into g
                        select new TransactionHistoryByProduct
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Quantity = g.Sum(qt => qt.Quantity),
                            Unit = g.Key.Unit,
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                             x.ItemName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataCumulative(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int year, string custCode, int userId)
        {
            var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
            var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.CustCode.Equals(custCode) &&
                             x.SalesBy.Equals(salesId)).AsEnumerable()
                             group so by new { so.Date.Year, so.Date.Month } into g
                             select new TransactionCumulative
                             {
                                 MonthInt = g.Key.Month,
                                 Month = GetMonth(g.Key.Month),
                                 Total = g.Sum(x => x.Total)
                             }).OrderBy(x => x.MonthInt).AsQueryable();

            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.Date.Year.Equals(year) && x.CustCode.Equals(custCode) &&
                              x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId)).AsEnumerable()
                              group so by new { so.Date.Year, so.Date.Month } into g
                              select new TransactionCumulative
                              {
                                  MonthInt = g.Key.Month,
                                  Month = GetMonth(g.Key.Month),
                                  Total = g.Sum(x => x.Total)
                              }).OrderBy(x => x.MonthInt).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile).AsEnumerable()
                        group so by new { so.Month, so.MonthInt } into g
                        select new TransactionCumulative
                        {
                            MonthInt = g.Key.MonthInt,
                            Month = g.Key.Month,
                            Total = g.Sum(x => x.Total)
                        }).OrderBy(x => x.MonthInt).AsQueryable();


            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        static String GetMonth(int month)
        {
            return month switch
            {
                1 => "Januari",
                2 => "Februari",
                3 => "Maret",
                4 => "April",
                5 => "Mei",
                6 => "Juni",
                7 => "Juli",
                8 => "Agustus",
                9 => "September",
                10 => "Oktober",
                11 => "November",
                12 => "Desember",
                _ => "Januari",
            };
        }

        public DataSourceResult GetDataByLog(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string custCode, int userId)
        {
            var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.CustCode.Equals(custCode) && x.SalesOrderCode.Equals(null))
                              join log in Db.MobileVisitLogs on so.VisitLogCode equals log.Code
                              group new { so, log } by new { so.Date, log.StartTime } into g
                              select new TransactionIndividual
                              {
                                  Date = g.Key.Date,
                                  Time = g.Key.StartTime,
                                  Total = g.Sum(x => x.so.Total)
                              }).OrderBy(x => x.Date).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.CustCode.Equals(custCode))
                             join sm in Db.MobileOrderHeaders on so.Code equals sm.SalesOrderCode into or
                             from p in or.DefaultIfEmpty()
                             join log in Db.MobileVisitLogs on p.VisitLogCode equals log.Code into ot
                             from q in ot.DefaultIfEmpty()
                             group new { so, p, q } by new { so.Date, Time = q.StartTime ?? null } into g
                             select new TransactionIndividual
                             {
                                 Date = g.Key.Date,
                                 Time = g.Key.Time,
                                 Total = g.Sum(x => x.so.Total)
                             }).OrderBy(x => x.Date).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.Date, so.Time } into g
                        select new TransactionIndividual
                        {
                            Date = g.Key.Date,
                            Time = g.Key.Time,
                            Total = g.Sum(x => x.Total)
                        }).OrderBy(x => x.Date).AsQueryable();


            return data.ToDataSourceResult(skip, take, filter, sort);
        }
    }
}
