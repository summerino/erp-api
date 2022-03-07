using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using System.Linq;

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
                                  Total = g.Sum(tl => tl.so.SubTotal)
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
                            Total = g.Sum(tl => tl.so.SubTotal)
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
                              join u in Db.UoMConversions on sod.UnitId equals u.Id
                              group new { so, sod, i, u } by new { so.Date, so.SalesBy, c.Code, sod.ItemId, i.Name, sod.UnitPrice, sod.UomId, u.UnitEquivalent } into g
                              select new TransactionItemDetail
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  CustomerId = g.Key.Code,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.UnitEquivalent,
                                  Price = g.Key.UnitPrice,
                                  Discount = g.Sum(dc => dc.sod.Disc),
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join c in Db.Customers on so.CustCode equals c.Code
                             join i in Db.Items on sod.ItemId equals i.Id
                             join u in Db.UoMConversions on sod.UnitId equals u.Id
                             group new { so, sod, i, u } by new { so.Date, so.SalesBy, c.Code, sod.ItemId, i.Name, sod.UnitPrice, sod.UomId, u.UnitEquivalent } into g
                             select new TransactionItemDetail
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 CustomerId = g.Key.Code,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.UnitEquivalent,
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
                              join u in Db.UoMConversions on sod.UnitId equals u.Id
                              group new { so, sod } by new { so.Date, so.SalesBy, c.Code, c.Name, sod.ItemId, sod.UnitPrice, sod.UomId, u.UnitEquivalent } into g
                              select new TransactionCustomerDetail
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  CustomerId = g.Key.Code,
                                  CustomerName = g.Key.Name,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.UnitEquivalent,
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join c in Db.Customers on so.CustCode equals c.Code
                             join u in Db.UoMConversions on sod.UnitId equals u.Id
                             group new { so, sod } by new { so.Date, so.SalesBy, c.Code, c.Name, sod.ItemId, sod.UnitPrice, sod.UomId, u.UnitEquivalent } into g
                             select new TransactionCustomerDetail
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 CustomerId = g.Key.Code,
                                 CustomerName = g.Key.Name,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.UnitEquivalent,
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
                                  Total = g.Sum(tl => tl.SubTotal)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             group so by new { so.SalesBy, so.Date } into g
                             select new TransactionHistoryByDate
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 Total = g.Sum(tl => tl.SubTotal)
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
                              join u in Db.UoMConversions on sod.UnitId equals u.Id
                              group new { so, sod, i, u } by new { so.SalesBy, so.Date, sod.ItemId, i.Name, sod.UomId, u.UnitEquivalent } into g
                              select new TransactionHistoryByProduct
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.UnitEquivalent,
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in Db.Items on sod.ItemId equals i.Id
                             join u in Db.UoMConversions on sod.UnitId equals u.Id
                             group new { so, sod, i, u } by new { so.SalesBy, so.Date, sod.ItemId, i.Name, sod.UomId, u.UnitEquivalent } into g
                             select new TransactionHistoryByProduct
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.UnitEquivalent,
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

        public IEnumerable<ItemSubGroupModel> GetSubGroup()
        {
            List<ItemSubGroupModel> data = new();

            var subGroups = (from G in Db.ItemGroups
                             join S in Db.ItemGroupSubGroups on G.Id equals S.ItemGroupId
                             where G.IsActive == true && S.ShowInMobile == true
                             select new ItemSubGroupModel
                             {
                                 GroupId = G.Id,
                                 GroupInitial = G.Initial,
                                 GroupName = G.Name,
                                 Name = S.Name,
                                 Value = S.Value
                             });
            var i = 0;
            foreach (ItemSubGroupModel sub in subGroups)
            {
                string[] values = sub.Value.Split(";");
                foreach (string value in values)
                {
                    data.Add(new ItemSubGroupModel
                    {
                        Id = ++i,
                        GroupId = sub.GroupId,
                        GroupInitial = sub.GroupInitial,
                        GroupName = sub.GroupName,
                        Name = sub.Name,
                        Value = value
                    });
                }
            };

            return data;
        }

        public DataSourceResult GetDataBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int groupId, string subGroup)
        {
            var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                              (x.SubGroup1.Equals(subGroup) || x.SubGroup2.Equals(subGroup) ||
                              x.SubGroup3.Equals(subGroup) || x.SubGroup4.Equals(subGroup) ||
                              x.SubGroup5.Equals(subGroup))) on sod.ItemId equals i.Id
                              join u in Db.UoMConversions on sod.UnitId equals u.Id
                              group new { so, sod } by new { so.SalesBy, so.Date, sod.ItemId } into g
                              select new TransactionHistoryBySubGroup
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                              (x.SubGroup1.Equals(subGroup) || x.SubGroup2.Equals(subGroup) ||
                              x.SubGroup3.Equals(subGroup) || x.SubGroup4.Equals(subGroup) ||
                              x.SubGroup5.Equals(subGroup))) on sod.ItemId equals i.Id
                             join u in Db.UoMConversions on sod.UnitId equals u.Id
                             group new { so, sod } by new { so.SalesBy, so.Date, sod.ItemId } into g
                             select new TransactionHistoryBySubGroup
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 Total = g.Sum(tl => tl.sod.Total)
                             }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.SalesId, so.Date } into g
                        select new TransactionHistoryBySubGroup
                        {
                            SalesId = g.Key.SalesId,
                            Date = g.Key.Date,
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetItemBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime date, int groupId, string subGroup)
        {
            var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);
            var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(date))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                              (x.SubGroup1.Equals(subGroup) || x.SubGroup2.Equals(subGroup) ||
                              x.SubGroup3.Equals(subGroup) || x.SubGroup4.Equals(subGroup) ||
                              x.SubGroup5.Equals(subGroup))) on sod.ItemId equals i.Id
                              join u in Db.UoMConversions on sod.UnitId equals u.Id
                              group new { so, sod, i, u } by new { so.SalesBy, sod.ItemId, i.Name, u.UnitEquivalent, sod.UnitPrice } into g
                              select new TransactionHistoryItemBySubGroup
                              {
                                  SalesId = g.Key.SalesBy,
                                  ItemId = g.Key.ItemId,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  Unit = g.Key.UnitEquivalent,
                                  Price = g.Key.UnitPrice,
                                  Discount = g.Sum(dc => dc.sod.Disc),
                                  Total = g.Sum(tl => tl.sod.Total)
                              }).AsQueryable();

            var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.Date.Equals(date))
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                              (x.SubGroup1.Equals(subGroup) || x.SubGroup2.Equals(subGroup) ||
                              x.SubGroup3.Equals(subGroup) || x.SubGroup4.Equals(subGroup) ||
                              x.SubGroup5.Equals(subGroup))) on sod.ItemId equals i.Id
                             join u in Db.UoMConversions on sod.UnitId equals u.Id
                             group new { so, sod, i, u } by new { so.SalesBy, sod.ItemId, i.Name, u.UnitEquivalent, sod.UnitPrice } into g
                             select new TransactionHistoryItemBySubGroup
                             {
                                 SalesId = g.Key.SalesBy,
                                 ItemId = g.Key.ItemId,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 Unit = g.Key.UnitEquivalent,
                                 Price = g.Key.UnitPrice,
                                 Discount = g.Sum(dc => dc.sod.Disc),
                                 Total = g.Sum(tl => tl.sod.Total)
                             }).AsQueryable();

            var data = (from so in dataOrder.Union(dataMobile)
                        group so by new { so.SalesId, so.ItemId, so.ItemName, so.Unit, so.Price } into g
                        select new TransactionHistoryItemBySubGroup
                        {
                            SalesId = g.Key.SalesId,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Quantity = g.Sum(qt => qt.Quantity),
                            Unit = g.Key.Unit,
                            Price = g.Key.Price,
                            Discount = g.Sum(dc => dc.Discount),
                            Total = g.Sum(tl => tl.Total)
                        }).AsQueryable();

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, int? groupId, int? subGroupId)
        {

            var subGroups = (from G in groupId == null ? Db.ItemGroups : Db.ItemGroups.Where(x => x.Id.Equals(groupId))
                             join S in subGroupId == null ? Db.ItemGroupSubGroups : Db.ItemGroupSubGroups.Where(y => y.Id.Equals(subGroupId)) on G.Id equals S.ItemGroupId
                             where G.IsActive == true && S.ShowInMobile == true
                             select new ItemSubGroupModel
                             {
                                 Id = S.Id,
                                 GroupId = G.Id,
                                 GroupInitial = G.Initial,
                                 GroupName = G.Name,
                                 Name = S.Name,
                                 Value = S.Value
                             }).AsEnumerable();

            var subGroup = (from ItemSubGroupModel sub in subGroups
                            let values = sub.Value.Split(";")
                            from string value in values
                            select new ItemSubGroupModel
                            {
                                Id = sub.Id,
                                GroupId = sub.GroupId,
                                GroupName = sub.GroupName,
                                GroupInitial = sub.GroupInitial,
                                Name = sub.Name,
                                Value = value
                            }).ToList();

            List<TransactionHistoryBySubGroupSummary> data = new List<TransactionHistoryBySubGroupSummary>();
            var groupName = "";
            var subGroupName = "";

            foreach (var sub in subGroup)
            {

                var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(date !=  DateTime.MinValue ? date : x.Date))
                                  join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                                  join it in Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                                  x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                                  x.SubGroup5.Contains(sub.Value)) on sod.ItemId equals it.Id
                                  group new { so, sod, it } by new { so.SalesBy } into g
                                  select new TransactionHistoryBySubGroupSummary
                                  {
                                      SalesId = g.Key.SalesBy,
                                      GroupId = sub.GroupId,
                                      GroupName = sub.GroupName,
                                      GroupInitial = sub.GroupInitial,
                                      SubGroupId = sub.Id,
                                      SubGroup = sub.Name,
                                      Total = g.Sum(tl => tl.sod.Total)
                                  }).ToList();

                foreach (var item in dataMobile)
                {
                    if (groupName != item.GroupName || subGroupName != item.SubGroup)
                    {
                        groupName = item.GroupName;
                        subGroupName = item.SubGroup;
                        data.Add(item);
                    }
                    else
                    {
                        data[data.Count - 1].Total += item.Total;
                    }

                }

            }

            return data.AsQueryable().ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataDetailBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, int groupId, int subGroupId)
        {
            var subGroups = (from G in Db.ItemGroups.Where(x => x.Id.Equals(groupId))
                             join S in Db.ItemGroupSubGroups.Where(y => y.Id.Equals(subGroupId)) on G.Id equals S.ItemGroupId
                             where G.IsActive == true && S.ShowInMobile == true
                             select new ItemSubGroupModel
                             {
                                 Id = S.Id,
                                 GroupId = G.Id,
                                 GroupInitial = G.Initial,
                                 GroupName = G.Name,
                                 Name = S.Name,
                                 Value = S.Value
                             }).AsEnumerable();

            var subGroup = (from ItemSubGroupModel sub in subGroups
                            let values = sub.Value.Split(";")
                            from string value in values
                            select new ItemSubGroupModel
                            {
                                Id= sub.Id,
                                GroupId = sub.GroupId,
                                GroupName = sub.GroupName,
                                GroupInitial = sub.GroupInitial,
                                Name = sub.Name,
                                Value = value
                            }).ToList();

            List<TransactionHistoryDetailBySubGroupSummary> data = new List<TransactionHistoryDetailBySubGroupSummary>();

            foreach (var sub in subGroup)
            {

                var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(date != DateTime.MinValue ? date : x.Date))
                                  join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                                  join it in Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) || x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) || x.SubGroup5.Contains(sub.Value)) on sod.ItemId equals it.Id
                                  group new { so, sod, it } by new { so.SalesBy } into g
                                  select new TransactionHistoryDetailBySubGroupSummary
                                  {
                                      SalesId = g.Key.SalesBy,
                                      DetailSubGroup = sub.Value,
                                      Total = g.Sum(tl => tl.sod.Total)
                                  }).ToList();


                foreach (var item in dataMobile)
                {
                    data.Add(item);
                }

            }

            return data.AsQueryable().ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetDataItemBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string detailSubGroup)
        {

            var data = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(date != DateTime.MinValue ? date : x.Date))
                        join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                        join it in Db.Items.Where(x => x.SubGroup1.Contains(detailSubGroup) || x.SubGroup2.Contains(detailSubGroup) || x.SubGroup3.Contains(detailSubGroup) || x.SubGroup4.Contains(detailSubGroup) || x.SubGroup5.Contains(detailSubGroup)) on sod.ItemId equals it.Id
                        join un in Db.UoMConversions on sod.UnitId equals un.Id
                        group new { so, sod, it, un } by new { so.SalesBy, sod.ItemId, it.Name, sod.UnitId, un.UnitEquivalent } into g
                        select new TransactionHistoryItemBySubGroupSummary
                        {
                            SalesId = g.Key.SalesBy,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.Name,
                            Unit = g.Key.UnitEquivalent,
                            Quantity = g.Sum(tl => tl.sod.Qty),
                            Total = g.Sum(tl => tl.sod.Total)
                        });

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<ItemGroup> GetItemGroup()
        {

            var data = Db.ItemGroups.Where(x=>x.IsActive).ToList();
           

            return data;
        }

        public IEnumerable<ItemGroupSubGroup> GetItemSubGroup(int groupId)
        {
            var data = Db.ItemGroupSubGroups.Where(x => x.ItemGroupId.Equals(groupId) && x.ShowInMobile).ToList();


            return data;
        }
    }
}
