using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ERP.Web.API.Domain.Services.Mobile.TransactionHistory;

public class TransactionHistoryService : ITransactionHistoryService
{
    protected TenantContext Db;

    public TransactionHistoryService(TenantContext db)
    {
        Db = db;
    }

    public DataSourceResult GetDataByCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search)
    {
        var customers = getCustomers();

        var dataMobile = from so in Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                         group new { so } by new
                         {
                             so.CustCode,
                             //cu.Name,
                             so.SalesBy,
                             so.Date
                         } into g
                         select new TransactionHistoryByCustomer
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             CustomerId = g.Key.CustCode,
                             //CustomerName = g.Key.Name,
                             Total = g.Sum(tl => tl.so.SubTotal)
                         };

        var dataOrder = (from so in Db.VwSalesOrderHeaders
                         group new { so } by new
                         {
                             so.CustCode,
                             so.SalesBy,
                             so.Date
                         } into g
                         select new TransactionHistoryByCustomer
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             CustomerId = g.Key.CustCode,
                             Total = g.Sum(tl => tl.so.SubTotal)
                         });

        var data = (from so in dataOrder.Union(dataMobile)
                    join cust in customers on so.CustomerId equals cust.Code
                    group new { so } by new { so.CustomerId, cust.Name, so.SalesId, so.Date } into g
                    select new TransactionHistoryByCustomer
                    {
                        SalesId = g.Key.SalesId,
                        Date = g.Key.Date,
                        CustomerId = g.Key.CustomerId,
                        CustomerName = g.Key.Name,
                        Total = g.Sum(tl => tl.so.Total)
                    }).OrderBy(x => x.Date).AsQueryable();

        //if (startDate != null && startDate.HasValue)
        //{
        //    data = data.Where(x => x.Date >= startDate && x.Date <= endDate);
        //}
        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate && x.Date <= endDate);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate);
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.CustomerName.Contains(search));
        }

        var result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetItemDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
    {
        var customers = getCustomers();

        var dataMobile = (from so in Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                          join sod in Db.VwMobileOrderDetails on so.Code equals sod.Code
                          //join c in Db.Customers on so.CustCode equals c.Code
                          join i in Db.Items on sod.ItemId equals i.Id
                          join u in Db.UoMConversions on sod.UnitId equals u.Id
                          group new { so, sod, i, u } by new
                          {
                              so.Date,
                              so.SalesBy,
                              so.CustCode,
                              sod.ItemId,
                              i.Name,
                              sod.UnitPrice,
                              sod.UomId,
                              u.UnitEquivalent
                          } into g
                          select new TransactionItemDetail
                          {
                              SalesId = g.Key.SalesBy,
                              Date = g.Key.Date,
                              CustomerId = g.Key.CustCode,
                              ItemId = g.Key.ItemId,
                              ItemName = g.Key.Name,
                              Quantity = g.Sum(qt => qt.sod.Qty),
                              //UomId = g.Key.UomId,
                              Unit = g.Key.UnitEquivalent,
                              Price = g.Key.UnitPrice,
                              Discount = g.Sum(dc => dc.sod.Disc),
                              Total = g.Sum(tl => tl.sod.Total)
                          });

        var dataOrder = (from so in Db.VwSalesOrderHeaders
                         join sod in Db.VwSalesOrderDetails on so.Code equals sod.Code
                         join i in Db.Items on sod.ItemId equals i.Id
                         join u in Db.UoMConversions on sod.UnitId equals u.Id
                         group new { so, sod, i, u } by new
                         {
                             so.Date,
                             so.SalesBy,
                             so.CustCode,
                             sod.ItemId,
                             i.Name,
                             sod.UnitPrice,
                             sod.UomId,
                             u.UnitEquivalent
                         } into g
                         select new TransactionItemDetail
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             CustomerId = g.Key.CustCode,
                             ItemId = g.Key.ItemId,
                             ItemName = g.Key.Name,
                             Quantity = g.Sum(qt => qt.sod.Qty),
                             //UomId = g.Key.UomId,
                             Unit = g.Key.UnitEquivalent,
                             Price = g.Key.UnitPrice,
                             Discount = g.Sum(dc => dc.sod.Disc),
                             Total = g.Sum(tl => tl.sod.Total)
                         });

        var data = (from so in dataOrder.Union(dataMobile)
                    join cu in customers on so.CustomerId equals cu.Code
                    group new { so } by new
                    {
                        so.SalesId,
                        so.Date,
                        CustomerId = cu.Code,
                        so.ItemId,
                        so.ItemName,
                        so.Unit,
                        so.Price
                    } into g
                    select new TransactionItemDetail
                    {
                        SalesId = g.Key.SalesId,
                        Date = g.Key.Date,
                        CustomerId = g.Key.CustomerId,
                        ItemId = g.Key.ItemId,
                        ItemName = g.Key.ItemName,
                        Quantity = g.Sum(qt => qt.so.Quantity),
                        //UomId = g.Key.UomId, 
                        Unit = g.Key.Unit,
                        Price = g.Key.Price,
                        Discount = g.Sum(dc => dc.so.Discount),
                        Total = g.Sum(tl => tl.so.Total)
                    }).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.ItemName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetCustomerDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
    {
        var customers = getCustomers();

        var dataMobile = (from so in Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                          join sod in Db.VwMobileOrderDetails on so.Code equals sod.Code
                          //join c in customers on so.CustCode equals c.Code
                          join u in Db.UoMConversions on sod.UnitId equals u.Id
                          join i in Db.Items on sod.ItemId equals i.Id
                          group new { so, sod, u, i } by new
                          {
                              so.Date,
                              so.SalesBy,
                              so.CustCode,
                              //c.Name,
                              sod.ItemId,
                              ItemName = i.Name,
                              sod.UnitPrice,
                              sod.UomId,
                              u.UnitEquivalent
                          } into g
                          select new TransactionCustomerDetail
                          {
                              SalesId = g.Key.SalesBy,
                              Date = g.Key.Date,
                              CustomerId = g.Key.CustCode,
                              //CustomerName = g.Key.Name,
                              ItemId = g.Key.ItemId,
                              ItemName = g.Key.ItemName,
                              Quantity = g.Sum(qt => qt.sod.Qty),
                              Unit = g.Key.UnitEquivalent,
                              Total = g.Sum(tl => tl.sod.Total)
                          });

        var dataOrder = (from so in Db.VwSalesOrderHeaders
                         join sod in Db.VwSalesOrderDetails on so.Code equals sod.Code
                         join u in Db.UoMConversions on sod.UnitId equals u.Id
                         join i in Db.Items on sod.ItemId equals i.Id
                         group new { so, sod, u, i } by new
                         {
                             so.Date,
                             so.SalesBy,
                             so.CustCode,
                             sod.ItemId,
                             ItemName = i.Name,
                             sod.UnitPrice,
                             sod.UomId,
                             u.UnitEquivalent
                         } into g
                         select new TransactionCustomerDetail
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             CustomerId = g.Key.CustCode,
                             ItemId = g.Key.ItemId,
                             ItemName = g.Key.ItemName,
                             Quantity = g.Sum(qt => qt.sod.Qty),
                             Unit = g.Key.UnitEquivalent,
                             Total = g.Sum(tl => tl.sod.Total)
                         });

        var data = (from so in dataOrder.Union(dataMobile)
                    join cu in customers on so.CustomerId equals cu.Code
                    group so by new
                    {
                        so.Date,
                        so.SalesId,
                        CustomerId = cu.Code,
                        CustomerName = cu.Name,
                        so.ItemId,
                        so.ItemName,
                        so.Unit
                    } into g
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
            data = data.Where(x => x.ItemName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetDataByDate(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate)
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

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate && x.Date <= endDate);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate);
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetDataByProduct(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search)
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

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate && x.Date <= endDate);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate);
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.ItemName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<TransactionHistoryByUnitProduct> GetDataByUnitProduct(int filterUnit, DateTime? startDate, DateTime? endDate, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        if (filterUnit == 1) // 1 = where isBaseUnit is true
        {
            var itemData = Db.Items.FromSqlRaw(@"SELECT * FROM Inventory.Item").AsQueryable();

            var uomData = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion").AsQueryable();

            var uomBase = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion where IsBaseUnit = 1").AsQueryable();

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomBase on uom.UomId equals u.UomId
                              group new { so, sod, i, uom } by new
                              {
                                  so.SalesBy,
                                  so.Date,
                                  sod.ItemId,
                                  i.Initial,
                                  i.Name,
                                  sod.UomId,
                                  uom.Id,
                                  sod.UnitId,
                                  uom.UnitEquivalent,
                                  uom.Seq,
                                  BaseSeq = u.Seq,
                                  uom.Conversion,
                              } into g
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = g.Key.SalesBy,
                                  Date = g.Key.Date,
                                  ItemId = g.Key.ItemId,
                                  ItemInitial = g.Key.Initial,
                                  ItemName = g.Key.Name,
                                  Quantity = g.Sum(qt => qt.sod.Qty),
                                  UomId = g.Key.UomId,
                                  UomToConvertId = g.Key.Id,
                                  UnitId = g.Key.UnitId,
                                  Unit = g.Key.UnitEquivalent,
                                  CurrentSeq = g.Key.Seq,
                                  BaseSeq = g.Key.BaseSeq,
                                  Conversion = g.Key.Conversion,
                                  Total = g.Sum(tl => tl.sod.Total)
                              });

            var orderData = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in itemData on sod.ItemId equals i.Id
                             join uom in uomData on sod.UnitId equals uom.Id
                             join u in uomBase on uom.UomId equals u.UomId
                             group new { so, sod, i, uom } by new
                             {
                                 so.SalesBy,
                                 so.Date,
                                 sod.ItemId,
                                 i.Initial,
                                 i.Name,
                                 sod.UomId,
                                 uom.Id,
                                 sod.UnitId,
                                 uom.UnitEquivalent,
                                 uom.Seq,
                                 BaseSeq = u.Seq,
                                 uom.Conversion,
                             } into g
                             select new TransactionHistoryByUnitProduct
                             {
                                 SalesId = g.Key.SalesBy,
                                 Date = g.Key.Date,
                                 ItemId = g.Key.ItemId,
                                 ItemInitial = g.Key.Initial,
                                 ItemName = g.Key.Name,
                                 Quantity = g.Sum(qt => qt.sod.Qty),
                                 UomId = g.Key.UomId,
                                 UomToConvertId = g.Key.Id,
                                 UnitId = g.Key.UnitId,
                                 Unit = g.Key.UnitEquivalent,
                                 CurrentSeq = g.Key.Seq,
                                 BaseSeq = g.Key.BaseSeq,
                                 Conversion = g.Key.Conversion,
                                 Total = g.Sum(tl => tl.sod.Total)
                             });

            var resultData = (from so in orderData.Union(mobileData)
                              group so by so into g
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = g.Key.SalesId,
                                  Date = g.Key.Date,
                                  ItemId = g.Key.ItemId,
                                  ItemInitial = g.Key.ItemInitial,
                                  ItemName = g.Key.ItemName,
                                  Quantity = g.Sum(qt => qt.Quantity),
                                  UomId = g.Key.UomId,
                                  UomToConvertId = g.Key.UomToConvertId,
                                  UnitId = g.Key.UnitId,
                                  Unit = g.Key.Unit,
                                  CurrentSeq = g.Key.CurrentSeq,
                                  BaseSeq = g.Key.BaseSeq,
                                  Conversion = g.Key.Conversion,
                                  Total = g.Sum(tl => tl.Total)
                              }).Where(x => x.SalesId.Equals(salesId)).OrderByDescending(x => x.Date).ToList();

            // count until uomId.Equals(uomBaseUnit)
            foreach (var rslt in resultData)
            {
                decimal count = 1;
                if (rslt.CurrentSeq > rslt.BaseSeq)
                {
                    List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                    for (int i = 0; i < conversions.Count; i++)
                    {
                        if (conversions[i].Seq >= (rslt.BaseSeq + 1) && conversions[i].Seq <= rslt.CurrentSeq)
                        {
                            count *= conversions[i].Conversion;
                        }
                    }

                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.IsBaseUnit) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.IsBaseUnit) select uom.UnitEquivalent).Single();
                    rslt.Total *= count;
                    rslt.Quantity *= count;
                }
            }

            var resultSum = resultData.GroupBy(so => new
            {
                so.SalesId,
                so.Date,
                so.ItemId,
                so.ItemInitial,
                so.ItemName,
                so.UomId,
                //so.UomToConvertId,
                so.UnitId,
                so.Unit,
                //so.CurrentSeq,
                so.BaseSeq,
                //so.Conversion,
            }).Select(y => new TransactionHistoryByUnitProduct
            {
                SalesId = y.Key.SalesId,
                Date = y.Key.Date,
                ItemId = y.Key.ItemId,
                ItemInitial = y.Key.ItemInitial,
                ItemName = y.Key.ItemName,
                Quantity = y.Sum(qt => qt.Quantity),
                UomId = y.Key.UomId,
                //UomToConvertId = y.Key.UomToConvertId,
                UnitId = y.Key.UnitId,
                Unit = y.Key.Unit,
                //CurrentSeq = y.Key.CurrentSeq,
                BaseSeq = y.Key.BaseSeq,
                //Conversion = y.Key.Conversion,
                Total = y.Sum(tl => tl.Total)
            }).ToList();

            // count until biggest uom
            foreach (var rslt in resultSum)
            {
                decimal count = 1;

                List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                for (int i = 0; i < conversions.Count; i++)
                {
                    count *= conversions[i].Conversion;
                }

                var maxSeq = Db.UoMConversions.Where(x => x.UomId == rslt.UomId).Max(x => x.Seq);

                rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Seq == maxSeq) select uom.Id).Single();
                rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Seq == maxSeq) select uom.UnitEquivalent).Single();
                rslt.Total /= count;
                rslt.Quantity /= count;
            }

            if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate && x.Date <= endDate);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate);

                return result;
            }
            else
            {
                var result = resultSum;

                return result;
            }
        }
        if (filterUnit == 2) // unit uomBuyId
        {
            var itemData = Db.Items.FromSqlRaw(@"SELECT * FROM Inventory.Item").AsQueryable();

            var uomData = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion").AsQueryable();

            var uomBuy = from uom in Db.UoMConversions
                         join item in Db.Items on uom.UomId equals item.UomId
                         where item.UomBuyId.Equals(uom.Id)
                         select uom;

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomBuy on uom.UomId equals u.UomId
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = so.SalesBy,
                                  Date = so.Date,
                                  ItemId = sod.ItemId,
                                  ItemInitial = i.Initial,
                                  ItemName = i.Name,
                                  Quantity = sod.Qty,
                                  UomId = sod.UomId,
                                  UomToConvertId = uom.Id,
                                  UnitId = sod.UnitId,
                                  Unit = uom.UnitEquivalent,
                                  CurrentSeq = uom.Seq,
                                  BaseSeq = u.Seq,
                                  Conversion = uom.Conversion,
                                  Total = sod.Total
                              });

            var orderData = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in itemData on sod.ItemId equals i.Id
                             join uom in uomData on sod.UnitId equals uom.Id
                             join u in uomBuy on uom.UomId equals u.UomId
                             select new TransactionHistoryByUnitProduct
                             {
                                 SalesId = so.SalesBy,
                                 Date = so.Date,
                                 ItemId = sod.ItemId,
                                 ItemInitial = i.Initial,
                                 ItemName = i.Name,
                                 Quantity = sod.Qty,
                                 UomId = sod.UomId,
                                 UomToConvertId = uom.Id,
                                 UnitId = sod.UnitId,
                                 Unit = uom.UnitEquivalent,
                                 CurrentSeq = uom.Seq,
                                 BaseSeq = u.Seq,
                                 Conversion = uom.Conversion,
                                 Total = sod.Total
                             });

            var resultData = (from so in orderData.Union(mobileData)
                              group so by so into g
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = g.Key.SalesId,
                                  Date = g.Key.Date,
                                  ItemId = g.Key.ItemId,
                                  ItemInitial = g.Key.ItemInitial,
                                  ItemName = g.Key.ItemName,
                                  Quantity = g.Sum(qt => qt.Quantity),
                                  UomId = g.Key.UomId,
                                  UomToConvertId = g.Key.UomToConvertId,
                                  UnitId = g.Key.UnitId,
                                  Unit = g.Key.Unit,
                                  CurrentSeq = g.Key.CurrentSeq,
                                  BaseSeq = g.Key.BaseSeq,
                                  Conversion = g.Key.Conversion,
                                  Total = g.Sum(tl => tl.Total)
                              }).Where(x => x.SalesId.Equals(salesId)).OrderByDescending(x => x.Date).ToList();

            foreach (var rslt in resultData)
            {
                decimal count = 1;
                if (rslt.CurrentSeq < rslt.BaseSeq)
                {
                    List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                    for (int i = conversions.Count - 1; i > 0; i--)
                    {
                        if (conversions[i].Seq <= (rslt.BaseSeq) && conversions[i].Seq > rslt.CurrentSeq)
                        {
                            count *= conversions[i].Conversion;
                        }
                    }

                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                    rslt.Total /= count;
                    rslt.Quantity /= count;
                }
                else if (rslt.CurrentSeq > rslt.BaseSeq)
                {
                    List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                    for (int i = rslt.BaseSeq; i < rslt.CurrentSeq; i++)
                    {
                        if (conversions[i].Seq >= (rslt.BaseSeq + 1) && conversions[i].Seq <= rslt.CurrentSeq)
                        {
                            count *= conversions[i].Conversion;
                        }
                    }

                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                    rslt.Total *= count;
                    rslt.Quantity *= count;
                }
                else
                {
                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                }
            }

            var resultSum = resultData.GroupBy(so => new
            {
                so.SalesId,
                so.Date,
                so.ItemId,
                so.ItemInitial,
                so.ItemName,
                so.UomId,
                //so.UomToConvertId,
                so.UnitId,
                so.Unit,
                //so.CurrentSeq,
                so.BaseSeq,
                //so.Conversion,
            }).Select(y => new TransactionHistoryByUnitProduct
            {
                SalesId = y.Key.SalesId,
                Date = y.Key.Date,
                ItemId = y.Key.ItemId,
                ItemInitial = y.Key.ItemInitial,
                ItemName = y.Key.ItemName,
                Quantity = y.Sum(qt => qt.Quantity),
                UomId = y.Key.UomId,
                //UomToConvertId = y.Key.UomToConvertId,
                UnitId = y.Key.UnitId,
                Unit = y.Key.Unit,
                //CurrentSeq = y.Key.CurrentSeq,
                BaseSeq = y.Key.BaseSeq,
                //Conversion = y.Key.Conversion,
                Total = y.Sum(tl => tl.Total)
            }).ToList();

            if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate && x.Date <= endDate);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate);

                return result;
            }
            else
            {
                var result = resultSum;

                return result;
            }
        }
        else // unit uomSellId,
        {
            var itemData = Db.Items.FromSqlRaw(@"SELECT * FROM Inventory.Item").AsQueryable();

            var uomData = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion").AsQueryable();

            var uomSell = from uom in Db.UoMConversions
                          join item in Db.Items on uom.UomId equals item.UomId
                          where item.UomSellId.Equals(uom.Id)
                          select uom;

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null))
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomSell on uom.UomId equals u.UomId
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = so.SalesBy,
                                  Date = so.Date,
                                  ItemId = sod.ItemId,
                                  ItemInitial = i.Initial,
                                  ItemName = i.Name,
                                  Quantity = sod.Qty,
                                  UomId = sod.UomId,
                                  UomToConvertId = uom.Id,
                                  UnitId = sod.UnitId,
                                  Unit = uom.UnitEquivalent,
                                  CurrentSeq = uom.Seq,
                                  BaseSeq = u.Seq,
                                  Conversion = uom.Conversion,
                                  Total = sod.Total
                              });

            var orderData = (from so in Db.SalesOrderHeaders
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in itemData on sod.ItemId equals i.Id
                             join uom in uomData on sod.UnitId equals uom.Id
                             join u in uomSell on uom.UomId equals u.UomId
                             select new TransactionHistoryByUnitProduct
                             {
                                 SalesId = so.SalesBy,
                                 Date = so.Date,
                                 ItemId = sod.ItemId,
                                 ItemInitial = i.Initial,
                                 ItemName = i.Name,
                                 Quantity = sod.Qty,
                                 UomId = sod.UomId,
                                 UomToConvertId = uom.Id,
                                 UnitId = sod.UnitId,
                                 Unit = uom.UnitEquivalent,
                                 CurrentSeq = uom.Seq,
                                 BaseSeq = u.Seq,
                                 Conversion = uom.Conversion,
                                 Total = sod.Total
                             });

            var resultData = (from so in orderData.Union(mobileData)
                              group so by so into g
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = g.Key.SalesId,
                                  Date = g.Key.Date,
                                  ItemId = g.Key.ItemId,
                                  ItemInitial = g.Key.ItemInitial,
                                  ItemName = g.Key.ItemName,
                                  Quantity = g.Sum(qt => qt.Quantity),
                                  UomId = g.Key.UomId,
                                  UomToConvertId = g.Key.UomToConvertId,
                                  UnitId = g.Key.UnitId,
                                  Unit = g.Key.Unit,
                                  CurrentSeq = g.Key.CurrentSeq,
                                  BaseSeq = g.Key.BaseSeq,
                                  Conversion = g.Key.Conversion,
                                  Total = g.Sum(tl => tl.Total)
                              }).Where(x => x.SalesId.Equals(salesId)).OrderByDescending(x => x.Date).ToList();

            foreach (var rslt in resultData)
            {
                decimal count = 1;
                if (rslt.CurrentSeq < rslt.BaseSeq)
                {
                    List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                    for (int i = conversions.Count - 1; i > 0; i--)
                    {
                        if (conversions[i].Seq <= (rslt.BaseSeq) && conversions[i].Seq > rslt.CurrentSeq)
                        {
                            count *= conversions[i].Conversion;
                        }
                    }

                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                    rslt.Total /= count;
                    rslt.Quantity /= count;
                }

                else if (rslt.CurrentSeq > rslt.BaseSeq)
                {
                    List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(rslt.UomId)).OrderBy(x => x.Seq).ToList();

                    for (int i = rslt.BaseSeq; i < rslt.CurrentSeq; i++)
                    {
                        if (conversions[i].Seq >= (rslt.BaseSeq + 1) && conversions[i].Seq <= rslt.CurrentSeq)
                        {
                            count *= conversions[i].Conversion;
                        }
                    }

                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                    rslt.Total *= count;
                    rslt.Quantity *= count;
                }
                else
                {
                    rslt.UnitId = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.Id).Single();
                    rslt.Unit = (from uom in Db.UoMConversions.Where(x => x.UomId == rslt.UomId && x.Id.Equals(Db.Items.Where(y => y.UomBuyId.Equals(x.Id)).Select(x => x.UomBuyId).Single())) select uom.UnitEquivalent).Single();
                }
            }

            var resultSum = resultData.GroupBy(so => new
            {
                so.SalesId,
                so.Date,
                so.ItemId,
                so.ItemInitial,
                so.ItemName,
                so.UomId,
                //so.UomToConvertId,
                so.UnitId,
                so.Unit,
                //so.CurrentSeq,
                so.BaseSeq,
                //so.Conversion,
            }).Select(y => new TransactionHistoryByUnitProduct
            {
                SalesId = y.Key.SalesId,
                Date = y.Key.Date,
                ItemId = y.Key.ItemId,
                ItemInitial = y.Key.ItemInitial,
                ItemName = y.Key.ItemName,
                Quantity = y.Sum(qt => qt.Quantity),
                UomId = y.Key.UomId,
                //UomToConvertId = y.Key.UomToConvertId,
                UnitId = y.Key.UnitId,
                Unit = y.Key.Unit,
                //CurrentSeq = y.Key.CurrentSeq,
                BaseSeq = y.Key.BaseSeq,
                //Conversion = y.Key.Conversion,
                Total = y.Sum(tl => tl.Total)
            }).ToList();

            if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate && x.Date <= endDate);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate);

                return result;
            }
            else
            {
                var result = resultSum;

                return result;
            }
        }
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

    public DataSourceResult GetDataBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int groupId, string subGroup, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);
        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId))
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

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId))
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

        if (filterStartDate != null && filterStartDate.HasValue && filterEndDate != null && filterEndDate.HasValue)
        {
            data = data.Where(x => x.Date >= filterStartDate && x.Date <= filterEndDate);
        }
        else if (filterStartDate != null && filterStartDate.HasValue && filterEndDate == null && !filterEndDate.HasValue)
        {
            data = data.Where(x => x.Date >= filterStartDate);
        }
        else if (filterStartDate == null && !filterStartDate.HasValue && filterEndDate != null && filterEndDate.HasValue)
        {
            data = data.Where(x => x.Date <= filterEndDate);
        }

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

    public DataSourceResult GetDataBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int? groupId, int? subGroupId, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);

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
            var dataMobile = (from so in filterStartDate == null ?
                                Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId))
                              : Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate) //.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(startDate != DateTime.MinValue ? startDate : x.Date)) // 
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join it in groupId == null ?
                                Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                                x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                                x.SubGroup5.Contains(sub.Value))
                              : Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                (x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                                x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                                x.SubGroup5.Contains(sub.Value)))
                              on sod.ItemId equals it.Id
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

            var dataOrder = (from so in filterStartDate == null ?
                               Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId))
                             : Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate) //.Where(x => x.SalesOrderCode.Equals(null) && x.Date.Equals(startDate != DateTime.MinValue ? startDate : x.Date)) // 
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join it in groupId == null ?
                               Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                               x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                               x.SubGroup5.Contains(sub.Value))
                             : Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                               (x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                               x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                               x.SubGroup5.Contains(sub.Value)))
                             on sod.ItemId equals it.Id
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

            var dataUnion = dataOrder.Union(dataMobile);

            foreach (var item in dataUnion)
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

        DataSourceResult result = data.AsQueryable().ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetDataDetailBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int filterGroupId, int groupId, int subGroupId, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);

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
                            Id = sub.Id,
                            GroupId = sub.GroupId,
                            GroupName = sub.GroupName,
                            GroupInitial = sub.GroupInitial,
                            Name = sub.Name,
                            Value = value
                        }).ToList();

        List<TransactionHistoryDetailBySubGroupSummary> data = new List<TransactionHistoryDetailBySubGroupSummary>();

        foreach (var sub in subGroup)
        {
            var targets = (from th in Db.SalesTargetHeaders
                           join td in Db.SalesTargetDetails.Where(x => x.ItemGroupId.Equals(groupId) && x.ItemSubGroupId.Equals(subGroupId) && x.SubGroup.Equals(sub.Value))
                           on th.Code equals td.Code
                           join ts in Db.SalesTargetSubjects on th.Code equals ts.Code
                           group new { th, td, ts } by new { td.ItemGroupId, td.ItemSubGroupId, td.SubGroup, ts.SalesmanId, th.StartDate, th.EndDate } into g
                           select new
                           {
                               SalesId = g.Key.SalesmanId,
                               StartDate = g.Key.StartDate,
                               EndDate = g.Key.EndDate,
                               SubGroup = g.Key.SubGroup,
                               Amount = g.Sum(tl => tl.td.Amount)
                           });

            var dataMobile = (from so in filterStartDate == null ?
                                Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId))
                              : Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate)
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join it in filterGroupId != 0 ?
                                Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                (x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                                x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                                x.SubGroup5.Contains(sub.Value)))
                              : Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                                x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                                x.SubGroup5.Contains(sub.Value))
                              on sod.ItemId equals it.Id
                              group new { so, sod, it } by new { so.SalesBy } into g
                              select new TransactionHistoryDetailBySubGroupSummary
                              {
                                  SalesId = g.Key.SalesBy,
                                  StartDate = targets.Where(x => x.SubGroup.Equals(sub.Value)).FirstOrDefault().StartDate,
                                  EndDate = targets.Where(x => x.SubGroup.Equals(sub.Value)).FirstOrDefault().EndDate,
                                  DetailSubGroup = sub.Value,
                                  Total = g.Sum(tl => tl.sod.Total),
                                  Target = targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault() == null ? 0 : targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault().Amount,
                                  PercentAchieved = targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault() == null ? 0 : g.Sum(tl => tl.sod.Total) / targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault().Amount * 100
                              }).ToList();

            var dataOrder = (from so in filterStartDate == null ?
                                Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId))
                              : Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate)
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join it in filterGroupId != 0 ?
                               Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                               (x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                               x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                               x.SubGroup5.Contains(sub.Value)))
                             : Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) || x.SubGroup2.Contains(sub.Value) ||
                               x.SubGroup3.Contains(sub.Value) || x.SubGroup4.Contains(sub.Value) ||
                               x.SubGroup5.Contains(sub.Value))
                             on sod.ItemId equals it.Id
                             group new { so, sod, it } by new { so.SalesBy } into g
                             select new TransactionHistoryDetailBySubGroupSummary
                             {
                                 SalesId = g.Key.SalesBy,
                                 StartDate = targets.Where(x => x.SubGroup.Equals(sub.Value)).FirstOrDefault().StartDate,
                                 EndDate = targets.Where(x => x.SubGroup.Equals(sub.Value)).FirstOrDefault().EndDate,
                                 DetailSubGroup = sub.Value,
                                 Total = g.Sum(tl => tl.sod.Total),
                                 Target = targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault() == null ? 0 : targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault().Amount,
                                 PercentAchieved = targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault() == null ? 0 : g.Sum(tl => tl.sod.Total) / targets.Where(x => x.SalesId.Equals(g.Key.SalesBy)).FirstOrDefault().Amount * 100
                             }).ToList();

            IEnumerable<TransactionHistoryDetailBySubGroupSummary> dataUnion = dataOrder.Union(dataMobile);

            var dataResult = (from du in dataUnion
                              group du by new
                              {
                                  du.SalesId,
                                  du.EndDate,
                                  du.StartDate,
                                  du.DetailSubGroup,
                                  du.Target
                              }
                              into g
                              select new TransactionHistoryDetailBySubGroupSummary
                              {
                                  SalesId = g.Key.SalesId,
                                  StartDate = g.Key.StartDate,
                                  EndDate = g.Key.EndDate,
                                  DetailSubGroup = g.Key.DetailSubGroup,
                                  Target = g.Key.Target,
                                  PercentAchieved = g.Sum(prc => prc.PercentAchieved),
                                  Total = g.Sum(tl => tl.Total)
                              });

            foreach (var item in dataResult)
            {
                data.Add(item);
            }
        }

        DataSourceResult result = data.AsQueryable().ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetDataItemBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, string detailSubGroup, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();

        //var data = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) //&& x.Date.Equals(date != DateTime.MinValue ? date : x.Date))
        var dataMobile = (from so in filterStartDate == null ?
                            Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId))
                          : Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate)
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

        var dataOrder = (from so in filterStartDate == null ?
                            Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId))
                          : Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Date >= filterStartDate && x.Date <= filterEndDate)
                         join sod in Db.SalesOrderDetails on so.Code equals sod.Code
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

        var dataUnion = dataOrder.Union(dataMobile);

        DataSourceResult result = dataUnion.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public IEnumerable<ItemGroup> GetItemGroup()
    {
        var data = Db.ItemGroups.Where(x => x.IsActive).ToList();

        return data;
    }

    public IEnumerable<ItemGroupSubGroup> GetItemSubGroup(int groupId)
    {
        var data = Db.ItemGroupSubGroups.Where(x => x.ItemGroupId.Equals(groupId) && x.ShowInMobile).ToList();

        return data;
    }

    public IQueryable<CustomerModel> getCustomers()
    {
        // customers and mobile customers
        var dataOriginal = (from cust in Db.VwCustomers
                            select new CustomerModel
                            {
                                Code = cust.Code,
                                Initial = cust.Initial,
                                Name = cust.Name,
                                TypeId = cust.TypeId,
                                TypeName = cust.TypeName,
                                CreditLimit = cust.CreditLimit,
                                Used = cust.CreditUsed,
                                Remaining = cust.CreditLimit - cust.CreditUsed,
                                AreaId1 = cust.AreaId1,
                                AreaId2 = cust.AreaId2,
                                AreaId3 = cust.AreaId3,
                                AreaId4 = cust.AreaId4,
                                AreaId5 = cust.AreaId5,
                                AreaName1 = cust.AreaName1,
                                AreaName2 = cust.AreaName2,
                                AreaName3 = cust.AreaName3,
                                AreaName4 = cust.AreaName4,
                                AreaName5 = cust.AreaName5,
                                Lat = cust.Lat,
                                Lng = cust.Lng,
                                InitialAddress = cust.InitialAddress,
                                Address1 = cust.Address1,
                                Address2 = cust.Address2,
                                Phone = cust.Phone,
                                Fax = cust.Fax,
                                ContactPerson = cust.ContactPerson,
                                IsActive = true,
                                UpdatedDate = cust.UpdatedDate,
                            });

        var dataMobile = (from custMobile in Db.VwMobileCustomers
                          where custMobile.CustCode == null && custMobile.Mark == "A"
                          select new CustomerModel
                          {
                              Code = custMobile.Code,
                              Initial = custMobile.Initial,
                              Name = custMobile.Name,
                              TypeId = custMobile.TypeId,
                              TypeName = custMobile.TypeName,
                              CreditLimit = (decimal)0.00,
                              Used = (decimal)0.00,
                              Remaining = (decimal)0.00,
                              AreaId1 = custMobile.AreaId1,
                              AreaId2 = custMobile.AreaId2,
                              AreaId3 = custMobile.AreaId3,
                              AreaId4 = custMobile.AreaId4,
                              AreaId5 = custMobile.AreaId5,
                              AreaName1 = custMobile.AreaName1,
                              AreaName2 = custMobile.AreaName2,
                              AreaName3 = custMobile.AreaName3,
                              AreaName4 = custMobile.AreaName4,
                              AreaName5 = custMobile.AreaName5,
                              Lat = custMobile.Lat,
                              Lng = custMobile.Lng,
                              InitialAddress = custMobile.InitialAddress,
                              Address1 = custMobile.Address1,
                              Address2 = custMobile.Address2,
                              Phone = custMobile.Phone,
                              Fax = custMobile.Fax,
                              ContactPerson = custMobile.ContactPerson,
                              IsActive = true,
                              UpdatedDate = custMobile.UpdatedDate,
                          });

        var dataCustomer = dataOriginal.Union(dataMobile).OrderBy(x => x.UpdatedDate).AsQueryable();

        return dataCustomer;
    }
}