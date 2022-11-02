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

    public DataSourceResult GetDataByCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search, int userId)
    {
        var customers = getCustomers();
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, Partial Shipment, Complete

        var dataMobile = (from mo in Db.VwMobileOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.SalesOrderCode.Equals(null) && x.Mark != "REJ")
                          join mod in Db.VwMobileOrderDetails on mo.Code equals mod.Code
                          group new { mo, mod } by new
                          {
                              mo.CustCode,
                              mo.SalesBy,
                              mo.Date
                          } into g
                          select new TransactionHistoryByCustomer
                          {
                              SalesId = g.Key.SalesBy,
                              Date = g.Key.Date,
                              CustomerId = g.Key.CustCode,
                              Total = g.Sum(tl => tl.mod.NettPrice * tl.mod.Qty)
                          }).ToList();

        var dataOrder = (from so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
                         join sod in Db.VwSalesOrderDetails on so.Code equals sod.Code
                         group new { so, sod } by new
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
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                         }).ToList();

        var dataClose = (from so in Db.VwSalesOrderHeaders.Where(x => x.Mark.Equals("CLS") && x.SalesBy.Equals(salesId))
                         join sod in Db.VwSalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
                         group new { so, sod } by new
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
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                         }).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose)
                    join cust in customers on so.CustomerId equals cust.Code
                    group new { so } by new
                    {
                        so.CustomerId,
                        cust.Name,
                        so.SalesId,
                        so.Date
                    } into g
                    select new TransactionHistoryByCustomer
                    {
                        SalesId = g.Key.SalesId,
                        Date = g.Key.Date,
                        CustomerId = g.Key.CustomerId,
                        CustomerName = g.Key.Name,
                        Total = g.Sum(tl => tl.so.Total)
                    }).OrderBy(x => x.Date).AsQueryable();

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate.Value.Date);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate.Value.Date);
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.CustomerName.Contains(search));
        }

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetItemDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, int userId)
    {
        var customers = getCustomers();
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
                          join sod in Db.VwMobileOrderDetails on so.Code equals sod.Code
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
                              Unit = g.Key.UnitEquivalent,
                              Price = g.Key.UnitPrice,
                              Discount = g.Sum(dc => dc.sod.Disc + dc.sod.FinalDiscHeader),
                              TaxAmount = g.Sum(tx => tx.sod.TaxAmount),
                              ExemptTaxAmount = g.Sum(etx => etx.sod.ExemptTaxAmount),
                              Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                          }).ToList();

        var dataOrder = (from so in Db.VwSalesOrderHeaders.Where(x => SOMarkIn.Contains(x.Mark) && x.SalesBy.Equals(salesId))
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
                             Unit = g.Key.UnitEquivalent,
                             Price = g.Key.UnitPrice,
                             Discount = g.Sum(dc => dc.sod.Disc + dc.sod.FinalDiscHeader),
                             TaxAmount = g.Sum(tx => tx.sod.TaxAmount),
                             ExemptTaxAmount = g.Sum(etx => etx.sod.ExemptTaxAmount),
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                         }).ToList();

        var dataClose = (from so in Db.VwSalesOrderHeaders.Where(x => x.Mark.Equals("CLS") && x.SalesBy.Equals(salesId))
                         join sod in Db.VwSalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
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
                             Quantity = g.Sum(qt => qt.sod.QtyDlv ?? 0),
                             Unit = g.Key.UnitEquivalent,
                             Price = g.Key.UnitPrice,
                             Discount = g.Sum(dc => dc.sod.Disc + dc.sod.FinalDiscHeader),
                             TaxAmount = g.Sum(tx => tx.sod.TaxAmount),
                             ExemptTaxAmount = g.Sum(etx => etx.sod.ExemptTaxAmount),
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                         }).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose)
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
                        Unit = g.Key.Unit,
                        Price = g.Key.Price,
                        Discount = g.Sum(dc => dc.so.Discount),
                        TaxAmount = g.Sum(tx => tx.so.TaxAmount),
                        ExemptTaxAmount = g.Sum(etx => etx.so.ExemptTaxAmount),
                        Total = g.Sum(tl => tl.so.Total)
                    }).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.ItemName.Contains(search));
        }

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetCustomerDetail(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, int userId)
    {
        var customers = getCustomers();
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
                          join sod in Db.VwMobileOrderDetails on so.Code equals sod.Code
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
                              Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                          }).ToList();

        var dataOrder = (from so in Db.VwSalesOrderHeaders.Where(x => SOMarkIn.Contains(x.Mark) && x.SalesBy.Equals(salesId))
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
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                         }).ToList();

        var dataClose = (from so in Db.VwSalesOrderHeaders.Where(x => x.Mark.Equals("CLS") && x.SalesBy.Equals(salesId))
                         join sod in Db.VwSalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
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
                             Quantity = g.Sum(qt => qt.sod.QtyDlv ?? 0),
                             Unit = g.Key.UnitEquivalent,
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                         }).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose)
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

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetDataByDate(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
                          join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                          group new { so, sod } by new
                          {
                              so.SalesBy,
                              so.Date
                          } into g
                          select new TransactionHistoryByDate
                          {
                              SalesId = g.Key.SalesBy,
                              Date = g.Key.Date,
                              Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                          }).ToList();

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
                         join sod in Db.VwSalesOrderDetails on so.Code equals sod.Code
                         group new { so, sod } by new
                         {
                             so.SalesBy,
                             so.Date
                         } into g
                         select new TransactionHistoryByDate
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                         }).ToList();

        var dataClose = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Mark.Equals("CLS"))
                         join sod in Db.VwSalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
                         group new { so, sod } by new
                         {
                             so.SalesBy,
                             so.Date
                         } into g
                         select new TransactionHistoryByDate
                         {
                             SalesId = g.Key.SalesBy,
                             Date = g.Key.Date,
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                         }).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose)
                    group so by new
                    {
                        so.SalesId,
                        so.Date
                    } into g
                    select new TransactionHistoryByDate
                    {
                        SalesId = g.Key.SalesId,
                        Date = g.Key.Date,
                        Total = g.Sum(tl => tl.Total)
                    }).AsQueryable();

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate.Value.Date);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate.Value.Date);
        }

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetDataByProduct(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? startDate, DateTime? endDate, string search, int userId)
    {
        // replaced to GetDataByUnitProduct

        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
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

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
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
            data = data.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);
        }
        else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
        {
            data = data.Where(x => x.Date >= startDate.Value.Date);
        }
        else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
        {
            data = data.Where(x => x.Date <= endDate.Value.Date);
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.ItemName.Contains(search));
        }

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public IEnumerable<TransactionHistoryByUnitProduct> GetDataByUnitProduct(int filterUnit, DateTime? startDate, DateTime? endDate, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        if (filterUnit == 1) // 1 = where isBaseUnit is true
        {
            var itemData = Db.Items.FromSqlRaw(@"SELECT * FROM Inventory.Item").AsQueryable();

            var uomData = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion").AsQueryable();

            var uomBase = Db.UoMConversions.FromSqlRaw(@"SELECT * FROM Inventory.UoMConversion where IsBaseUnit = 1").AsQueryable();

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomBase on uom.UomId equals u.UomId
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
                                  Total = (sod.NettPrice * sod.Qty)
                                  //Total = g.Sum(tl => tl.sod.Total)
                              }).ToList();

            var orderData = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
                             join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                             join i in itemData on sod.ItemId equals i.Id
                             join uom in uomData on sod.UnitId equals uom.Id
                             join u in uomBase on uom.UomId equals u.UomId
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
                                 Total = (sod.NettPrice * sod.Qty)
                                 //Total = g.Sum(tl => tl.sod.Total)
                             }).ToList();

            var orderClose = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Mark.Equals("CLS"))
                              join sod in Db.SalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomBase on uom.UomId equals u.UomId
                              select new TransactionHistoryByUnitProduct
                              {
                                  SalesId = so.SalesBy,
                                  Date = so.Date,
                                  ItemId = sod.ItemId,
                                  ItemInitial = i.Initial,
                                  ItemName = i.Name,
                                  Quantity = sod.QtyDlv ?? 0,
                                  UomId = sod.UomId,
                                  UomToConvertId = uom.Id,
                                  UnitId = sod.UnitId,
                                  Unit = uom.UnitEquivalent,
                                  CurrentSeq = uom.Seq,
                                  BaseSeq = u.Seq,
                                  Conversion = uom.Conversion,
                                  Total = (sod.NettPrice * sod.QtyDlv ?? 0)
                                  //Total = sod.Total
                              }).ToList();

            var resultData = (from so in orderData.Union(mobileData).Union(orderClose)
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
                              }).OrderByDescending(x => x.Date).ToList();

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
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate.Value.Date);

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

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
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
                                  Total = (sod.NettPrice * sod.Qty)
                                  //Total = sod.Total
                              }).ToList();

            var orderData = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
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
                                 Total = (sod.NettPrice * sod.Qty)
                                 //Total = sod.Total
                             }).ToList();

            var orderClose = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Mark.Equals("CLS"))
                              join sod in Db.SalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
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
                                  Quantity = sod.QtyDlv ?? 0,
                                  UomId = sod.UomId,
                                  UomToConvertId = uom.Id,
                                  UnitId = sod.UnitId,
                                  Unit = uom.UnitEquivalent,
                                  CurrentSeq = uom.Seq,
                                  BaseSeq = u.Seq,
                                  Conversion = uom.Conversion,
                                  Total = (sod.NettPrice * sod.QtyDlv ?? 0)
                                  //Total = sod.Total
                              }).ToList();

            var resultData = (from so in orderData.Union(mobileData).Union(orderClose)
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
                              }).OrderByDescending(x => x.Date).ToList();

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
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate.Value.Date);

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

            var mobileData = (from so in Db.MobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) && x.SalesBy.Equals(salesId) && x.Mark != "REJ")
                              join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                              join i in itemData on sod.ItemId equals i.Id
                              join uom in uomData on sod.UnitId equals uom.Id
                              join u in uomSell on uom.UomId equals u.UomId
                              //group new { so, sod, i, uom, u } by new
                              //{
                              //    so.SalesBy,
                              //    so.Date,
                              //    sod.ItemId,
                              //    i.Initial,
                              //    i.Name,
                              //    sod.UomId,
                              //    uom.Id,
                              //    sod.UnitId,
                              //    uom.UnitEquivalent,
                              //    uom.Seq,
                              //    BaseSeq = u.Seq,
                              //    uom.Conversion,
                              //} into g
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
                                  Total = (sod.NettPrice * sod.Qty)
                                  //Total = sod.Total
                              }).ToList();

            var orderData = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && SOMarkIn.Contains(x.Mark))
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
                                 Total = (sod.NettPrice * sod.Qty)
                                 //Total = sod.Total
                             }).ToList();

            var orderClose = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.Mark.Equals("CLS"))
                              join sod in Db.SalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
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
                                  Quantity = sod.QtyDlv ?? 0,
                                  UomId = sod.UomId,
                                  UomToConvertId = uom.Id,
                                  UnitId = sod.UnitId,
                                  Unit = uom.UnitEquivalent,
                                  CurrentSeq = uom.Seq,
                                  BaseSeq = u.Seq,
                                  Conversion = uom.Conversion,
                                  Total = (sod.NettPrice * sod.QtyDlv ?? 0)
                                  //Total = sod.Total
                              }).ToList();

            var resultData = (from so in orderData.Union(mobileData).Union(orderClose)
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
                              }).OrderByDescending(x => x.Date).ToList();

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
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date && x.Date <= endDate.Value.Date);

                return result;
            }
            else if (startDate != null && startDate.HasValue && endDate == null && !endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date >= startDate.Value.Date);

                return result;
            }
            else if (startDate == null && !startDate.HasValue && endDate != null && endDate.HasValue)
            {
                var result = resultSum.Where(x => x.Date <= endDate.Value.Date);

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
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.Date.Year.Equals(year) &&
                                                                      x.CustCode.Equals(custCode) &&
                                                                      x.SalesOrderCode.Equals(null) &&
                                                                      x.SalesBy.Equals(salesId) &&
                                                                      x.Mark != "REJ").AsEnumerable()
                          join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                          group new { so, sod } by new { so.Date.Year, so.Date.Month } into g
                          select new TransactionCumulative
                          {
                              MonthInt = g.Key.Month,
                              Month = GetMonth(g.Key.Month),
                              Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                              //Total = g.Sum(x => x.Total)
                          }).OrderBy(x => x.MonthInt).ToList();

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.Date.Year.Equals(year) &&
                                                                    x.CustCode.Equals(custCode) &&
                                                                    x.SalesBy.Equals(salesId) &&
                                                                    SOMarkIn.Contains(x.Mark)).AsEnumerable()
                         join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                         group new { so, sod } by new { so.Date.Year, so.Date.Month } into g
                         select new TransactionCumulative
                         {
                             MonthInt = g.Key.Month,
                             Month = GetMonth(g.Key.Month),
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                             //Total = g.Sum(x => x.Total)
                         }).OrderBy(x => x.MonthInt).ToList();

        var dataClose = (from so in Db.SalesOrderHeaders.Where(x => x.Date.Year.Equals(year) &&
                                                                    x.CustCode.Equals(custCode) &&
                                                                    x.SalesBy.Equals(salesId) &&
                                                                    x.Mark.Equals("CLS")).AsEnumerable()
                         join sod in Db.VwSalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
                         group new { so, sod } by new { so.Date.Year, so.Date.Month } into g
                         select new TransactionCumulative
                         {
                             MonthInt = g.Key.Month,
                             Month = GetMonth(g.Key.Month),
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                             //Total = g.Sum(x => x.Total)
                         }).OrderBy(x => x.MonthInt).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose).AsEnumerable()
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
        string[] SOMarkIn = new string[] { "A", "PS", "CMP" }; // Active, PS, Complete

        var dataMobile = (from so in Db.MobileOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.CustCode.Equals(custCode) && x.SalesOrderCode.Equals(null) && x.Mark != "REJ")
                          join sod in Db.MobileOrderDetails on so.Code equals sod.Code
                          join log in Db.MobileVisitLogs on so.VisitLogCode equals log.Code
                          group new { so, sod, log } by new { so.Date, log.StartTime } into g
                          select new TransactionIndividual
                          {
                              Date = g.Key.Date,
                              Time = g.Key.StartTime,
                              Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                              //Total = g.Sum(x => x.so.Total)
                          }).OrderBy(x => x.Date).ToList();

        var dataOrder = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.CustCode.Equals(custCode) && SOMarkIn.Contains(x.Mark))
                         join sod in Db.SalesOrderDetails on so.Code equals sod.Code
                         join sm in Db.MobileOrderHeaders on so.Code equals sm.SalesOrderCode into or
                         from p in or.DefaultIfEmpty()
                         join log in Db.MobileVisitLogs on p.VisitLogCode equals log.Code into ot
                         from q in ot.DefaultIfEmpty()
                         group new { so, sod, p, q } by new { so.Date, Time = q.StartTime ?? null } into g
                         select new TransactionIndividual
                         {
                             Date = g.Key.Date,
                             Time = g.Key.Time,
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.Qty)
                             //Total = g.Sum(x => x.so.Total)
                         }).OrderBy(x => x.Date).ToList();

        var dataClose = (from so in Db.SalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId) && x.CustCode.Equals(custCode) && x.Mark.Equals(x.Mark))
                         join sod in Db.SalesOrderDetails.Where(x => x.QtyDlv > 0) on so.Code equals sod.Code
                         join sm in Db.MobileOrderHeaders on so.Code equals sm.SalesOrderCode into or
                         from p in or.DefaultIfEmpty()
                         join log in Db.MobileVisitLogs on p.VisitLogCode equals log.Code into ot
                         from q in ot.DefaultIfEmpty()
                         group new { so, sod, p, q } by new { so.Date, Time = q.StartTime ?? null } into g
                         select new TransactionIndividual
                         {
                             Date = g.Key.Date,
                             Time = g.Key.Time,
                             Total = g.Sum(tl => tl.sod.NettPrice * tl.sod.QtyDlv ?? 0)
                             //Total = g.Sum(x => x.so.Total)
                         }).OrderBy(x => x.Date).ToList();

        var data = (from so in dataOrder.Union(dataMobile).Union(dataClose)
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
        //string[] SOMarkIn = new string[] { "A", "PS", "CMP", ""CLS" }; // Active, Partial Shipment, Complete, Close
        //string[] SOMarkNotIn = new string[] { "V", "OL" }; // Void, Over Limit
        string[] SIMarkIn = new string[] { "A", "PP", "CMP" }; // Active, Partial Payment

        var mobileHeaderData = (from mo in Db.VwMobileOrderHeaders.Where(x => x.Mark != "REJ" &&
                                                                              x.SalesBy.Equals(salesId) &&
                                                                              x.SalesOrderCode.Equals(null))
                                join mod in Db.VwMobileOrderDetails on mo.Code equals mod.Code
                                join it in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                      (x.SubGroup1.Equals(subGroup) ||
                                                       x.SubGroup2.Equals(subGroup) ||
                                                       x.SubGroup3.Equals(subGroup) ||
                                                       x.SubGroup4.Equals(subGroup) ||
                                                       x.SubGroup5.Equals(subGroup))) on mod.ItemId equals it.Id
                                join u in Db.UoMConversions on mod.UnitId equals u.Id
                                select new
                                {
                                    SalesId = mo.SalesBy,
                                    mo.Date,
                                    mod.Total,
                                    TotalNettPrice = mod.NettPrice * mod.Qty,
                                });

        var mobileData = (from mh in mobileHeaderData
                          group mh by new
                          {
                              mh.Date,
                              mh.SalesId,
                          } into g
                          select new
                          {
                              g.Key.Date,
                              g.Key.SalesId,
                              Total = g.Sum(x => x.Total),
                              TotalNettPrice = g.Sum(x => x.TotalNettPrice),
                          }).ToList();

        var salesHeaderData = (from si in Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark))
                               join sid in Db.SalesInvoiceDetails on si.Code equals sid.Code
                               join sd in Db.VwSalesDeliveryHeaders on sid.DoCode equals sd.Code
                               join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
                               join so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId)) on si.SoCode equals so.Code
                               join it in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                        (x.SubGroup1.Equals(subGroup) ||
                                                         x.SubGroup2.Equals(subGroup) ||
                                                         x.SubGroup3.Equals(subGroup) ||
                                                         x.SubGroup4.Equals(subGroup) ||
                                                         x.SubGroup5.Equals(subGroup))) on sdd.ItemId equals it.Id
                               join u in Db.UoMConversions on sdd.UnitId equals u.Id
                               select new
                               {
                                   SalesId = so.SalesBy,
                                   si.Date,
                                   GrossAmount = sdd.UnitPrice,
                                   Disc = sdd.Disc,
                                   DiscHeader = sdd.FinalDiscHeader,
                                   SubTotal = sdd.UnitPrice - sdd.Disc - sdd.FinalDiscHeader,
                                   sdd.Dpp,
                                   sdd.TaxAmount,
                                   sdd.ExemptTaxAmount,
                                   sdd.Total,
                                   TotalNettPrice = sdd.NettPrice * sdd.Qty,
                               });

        var salesData = (from sh in salesHeaderData
                         group sh by new
                         {
                             sh.Date,
                             sh.SalesId,
                         } into g
                         select new
                         {
                             g.Key.Date,
                             g.Key.SalesId,
                             Total = g.Sum(x => x.Total),
                             TotalNettPrice = g.Sum(x => x.TotalNettPrice),
                         }).ToList();

        var data = (from so in salesData.Union(mobileData)
                    group new { so } by new
                    {
                        so.Date,
                        so.SalesId
                    } into g
                    select new TransactionHistoryBySubGroup
                    {
                        Date = g.Key.Date,
                        SalesId = g.Key.SalesId,
                        Total = g.Sum(x => x.so.TotalNettPrice),
                    }).AsQueryable();

        if (filterStartDate != null && filterStartDate.HasValue && filterEndDate != null && filterEndDate.HasValue)
        {
            data = data.Where(x => x.Date >= filterStartDate.Value.Date && x.Date <= filterEndDate.Value.Date);
        }
        else if (filterStartDate != null && filterStartDate.HasValue)
        {
            data = data.Where(x => x.Date >= filterStartDate.Value.Date);
        }
        else if (filterEndDate != null && filterEndDate.HasValue)
        {
            data = data.Where(x => x.Date <= filterEndDate.Value.Date);
        }

        data = data.OrderByDescending(x => x.Date).AsQueryable();

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetItemBySubGroup(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime date, int groupId, string subGroup, int userId)
    {
        var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        //string[] SOMarkIn = new string[] { "A", "PS", "CMP", ""CLS", "" }; // Activartial ehipment, Complete, Close, PS
        //string[] SOMarkNotIn = new string[] { "V", "OL" }; // Void, Over Limit 
        string[] SIMarkIn = new string[] { "A", "PP", "CMP" }; // Active, Partial Payment

        var mobileHeaderData = (from mo in Db.VwMobileOrderHeaders.Where(x => x.Date.Equals(date) &&
                                                                              x.Mark != "REJ" &&
                                                                              x.SalesBy.Equals(salesId) &&
                                                                              x.SalesOrderCode.Equals(null))
                                join mod in Db.VwMobileOrderDetails on mo.Code equals mod.Code
                                join it in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                               (x.SubGroup1.Equals(subGroup) ||
                                                                x.SubGroup2.Equals(subGroup) ||
                                                                x.SubGroup3.Equals(subGroup) ||
                                                                x.SubGroup4.Equals(subGroup) ||
                                                                x.SubGroup5.Equals(subGroup))) on mod.ItemId equals it.Id
                                join u in Db.UoMConversions on mod.UnitId equals u.Id
                                select new
                                {
                                    SalesId = mo.SalesBy,
                                    ItemId = it.Id,
                                    ItemName = it.Name,
                                    u.UnitEquivalent,
                                    mod.Qty,
                                    Price = mod.UnitPrice,
                                    Disc = mod.Disc,
                                    mod.FinalDiscHeader,
                                    mod.TaxAmount,
                                    mod.ExemptTaxAmount,
                                    mod.NettPrice,
                                    mod.Total,
                                    TotalNettPrice = mod.NettPrice * mod.Qty,
                                });

        var mobileData = (from mh in mobileHeaderData
                          group new { mh } by new
                          {
                              mh.SalesId,
                              mh.ItemId,
                              mh.ItemName,
                              mh.UnitEquivalent,
                              mh.Price
                          } into g
                          select new
                          {
                              g.Key.SalesId,
                              ItemId = g.Key.ItemId,
                              ItemName = g.Key.ItemName,
                              Unit = g.Key.UnitEquivalent,
                              Price = g.Key.Price,
                              Quantity = g.Sum(qt => qt.mh.Qty),
                              Discount = g.Sum(dc => dc.mh.Disc + dc.mh.FinalDiscHeader),
                              TaxAmount = g.Sum(tx => tx.mh.TaxAmount),
                              ExemptTaxAmount = g.Sum(etx => etx.mh.ExemptTaxAmount),
                              Total = g.Sum(tl => tl.mh.Total),
                              TotalNettPrice = g.Sum(x => x.mh.TotalNettPrice),
                          }).ToList();

        var salesHeaderData = (from si in Db.VwSalesInvoiceHeaders.Where(x => x.Date.Equals(date) &&
                                                                              SIMarkIn.Contains(x.Mark))
                               join sid in Db.SalesInvoiceDetails on si.Code equals sid.Code
                               join sd in Db.VwSalesDeliveryHeaders on sid.DoCode equals sd.Code
                               join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
                               join so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId)) on si.SoCode equals so.Code
                               join it in Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                              (x.SubGroup1.Equals(subGroup) ||
                                                              x.SubGroup2.Equals(subGroup) ||
                                                              x.SubGroup3.Equals(subGroup) ||
                                                              x.SubGroup4.Equals(subGroup) ||
                                                              x.SubGroup5.Equals(subGroup))) on sdd.ItemId equals it.Id
                               join u in Db.UoMConversions on sdd.UnitId equals u.Id
                               select new
                               {
                                   SalesId = so.SalesBy,
                                   ItemId = it.Id,
                                   ItemName = it.Name,
                                   u.UnitEquivalent,
                                   sdd.Qty,
                                   Price = sdd.UnitPrice,
                                   sdd.Disc,
                                   sdd.FinalDiscHeader,
                                   sdd.TaxAmount,
                                   sdd.ExemptTaxAmount,
                                   sdd.NettPrice,
                                   sdd.Total,
                                   TotalNettPrice = sdd.NettPrice * sdd.Qty,
                               });

        var salesData = (from so in salesHeaderData
                         group new
                         {
                             so
                         } by new
                         {
                             so.SalesId,
                             so.ItemId,
                             so.ItemName,
                             so.UnitEquivalent,
                             so.Price
                         } into g
                         select new
                         {
                             g.Key.SalesId,
                             ItemId = g.Key.ItemId,
                             ItemName = g.Key.ItemName,
                             Unit = g.Key.UnitEquivalent,
                             Price = g.Key.Price,
                             Quantity = g.Sum(qt => qt.so.Qty),
                             Discount = g.Sum(dc => dc.so.Disc + dc.so.FinalDiscHeader),
                             TaxAmount = g.Sum(tx => tx.so.TaxAmount),
                             ExemptTaxAmount = g.Sum(etx => etx.so.ExemptTaxAmount),
                             Total = g.Sum(tl => tl.so.Total),
                             TotalNettPrice = g.Sum(x => x.so.TotalNettPrice),
                         }).ToList();

        var data = (from so in salesData.Union(mobileData)
                    group new { so } by new
                    {
                        so.SalesId,
                        so.ItemId,
                        so.ItemName,
                        so.Unit,
                        so.Price
                    } into g
                    select new TransactionHistoryItemBySubGroup
                    {
                        SalesId = g.Key.SalesId,
                        ItemId = g.Key.ItemId,
                        ItemName = g.Key.ItemName,
                        Unit = g.Key.Unit,
                        Price = g.Key.Price,
                        Quantity = g.Sum(qt => qt.so.Quantity),
                        Discount = g.Sum(dc => dc.so.Discount),
                        TaxAmount = g.Sum(tx => tx.so.TaxAmount),
                        ExemptTaxAmount = g.Sum(etx => etx.so.ExemptTaxAmount),
                        Total = g.Sum(tl => tl.so.TotalNettPrice)
                    }).AsQueryable();

        DataSourceResult result = data.ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetDataBySubGroupSummary(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? filterStartDate, DateTime? filterEndDate, int? groupId, int? subGroupId, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var categories = Db.ItemCategories.Where(x => x.GroupId.Equals(groupId)).Select(y => y.Id);
        //string[] SOMarkIn = new string[] { "A", "PS", "CMP", ""CLS" }; // Active, Partial Shipment, Complete, Close
        //string[] SOMarkNotIn = new string[] { "V", "OL" }; // Void, Over Limit
        string[] SIMarkIn = new string[] { "A", "PP", "CMP" }; // Active, Partial Payment

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
            var mobileHeaderData = (from mo in filterStartDate == null ?
                                     Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                        x.SalesBy.Equals(salesId) &&
                                                                        x.Mark != "REJ")
                                   : Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                        x.SalesBy.Equals(salesId) &&
                                                                        x.Mark != "REJ" &&
                                                                        x.Date >= filterStartDate &&
                                                                        x.Date <= filterEndDate)
                                    join mo_d in Db.MobileOrderDetails on mo.Code equals mo_d.Code
                                    join it in groupId == null ?
                                      Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) ||
                                                          x.SubGroup2.Contains(sub.Value) ||
                                                          x.SubGroup3.Contains(sub.Value) ||
                                                          x.SubGroup4.Contains(sub.Value) ||
                                                          x.SubGroup5.Contains(sub.Value))
                                    : Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                         (x.SubGroup1.Contains(sub.Value) ||
                                                          x.SubGroup2.Contains(sub.Value) ||
                                                          x.SubGroup3.Contains(sub.Value) ||
                                                          x.SubGroup4.Contains(sub.Value) ||
                                                          x.SubGroup5.Contains(sub.Value))) on mo_d.ItemId equals it.Id
                                    join u in Db.UoMConversions on mo_d.UnitId equals u.Id
                                    select new
                                    {
                                        SalesId = mo.SalesBy,
                                        TotalNettPrice = mo_d.NettPrice * mo_d.Qty
                                    });

            var mobileData = (from mo in mobileHeaderData
                              group new
                              {
                                  mo,
                              } by new { mo.SalesId } into g
                              select new TransactionHistoryBySubGroupSummary
                              {
                                  SalesId = g.Key.SalesId,
                                  GroupId = sub.GroupId,
                                  GroupName = sub.GroupName,
                                  GroupInitial = sub.GroupInitial,
                                  SubGroupId = sub.Id,
                                  SubGroup = sub.Name,
                                  Total = g.Sum(tl => tl.mo.TotalNettPrice)
                              }).ToList();

            var salesHeaderData = (from si in filterStartDate == null ?
                                     Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark))
                                   : Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark) &&
                                                                         x.Date >= filterStartDate &&
                                                                         x.Date <= filterEndDate)
                                   join sid in Db.SalesInvoiceDetails on si.Code equals sid.Code
                                   join sd in Db.VwSalesDeliveryHeaders on sid.DoCode equals sd.Code
                                   join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
                                   join so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId)) on si.SoCode equals so.Code
                                   join it in groupId == null ?
                                     Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) ||
                                                         x.SubGroup2.Contains(sub.Value) ||
                                                         x.SubGroup3.Contains(sub.Value) ||
                                                         x.SubGroup4.Contains(sub.Value) ||
                                                         x.SubGroup5.Contains(sub.Value))
                                   : Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                        (x.SubGroup1.Contains(sub.Value) ||
                                                         x.SubGroup2.Contains(sub.Value) ||
                                                         x.SubGroup3.Contains(sub.Value) ||
                                                         x.SubGroup4.Contains(sub.Value) ||
                                                         x.SubGroup5.Contains(sub.Value))) on sdd.ItemId equals it.Id
                                   join u in Db.UoMConversions on sdd.UnitId equals u.Id
                                   select new
                                   {
                                       SalesId = so.SalesBy,
                                       TotalNettPrice = sdd.NettPrice * sdd.Qty
                                   });

            var salesData = (from so in salesHeaderData
                             group new
                             {
                                 so,
                             } by new { so.SalesId } into g
                             select new TransactionHistoryBySubGroupSummary
                             {
                                 SalesId = g.Key.SalesId,
                                 GroupId = sub.GroupId,
                                 GroupName = sub.GroupName,
                                 GroupInitial = sub.GroupInitial,
                                 SubGroupId = sub.Id,
                                 SubGroup = sub.Name,
                                 Total = g.Sum(tl => tl.so.TotalNettPrice)
                             }).ToList();

            var dataUnion = salesData.Union(mobileData);

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
        //string[] SOMarkIn = new string[] { "A", "PS", "CMP", ""CLS" }; // Active, Partial Shipment, Complete, Close
        //string[] SOMarkNotIn = new string[] { "V", "OL" }; // Void, Over Limit
        string[] SIMarkIn = new string[] { "A", "PP", "CMP" }; // Active, Partial Payment

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
            var targets = (from th in filterStartDate == null ?
                             Db.SalesTargetHeaders.Where(x => x.Mark.Equals("A"))
                           : Db.SalesTargetHeaders.Where(x => x.Mark.Equals("A") &&
                                                              ((x.StartDate >= filterStartDate && x.StartDate <= filterEndDate) ||
                                                               (x.EndDate >= filterStartDate && x.EndDate <= filterEndDate)))
                           join td in Db.SalesTargetDetails.Where(x => x.ItemGroupId.Equals(groupId) &&
                                                                       x.ItemSubGroupId.Equals(subGroupId) &&
                                                                       x.SubGroup.Equals(sub.Value)) on th.Code equals td.Code
                           join ts in Db.SalesTargetSubjects on th.Code equals ts.Code
                           group new
                           {
                               th,
                               td,
                               ts
                           } by new
                           {
                               td.ItemGroupId,
                               td.ItemSubGroupId,
                               td.SubGroup,
                               ts.SalesmanId
                           } into g
                           select new
                           {
                               SalesId = g.Key.SalesmanId,
                               SubGroup = g.Key.SubGroup,
                               Amount = g.Sum(tl => tl.td.Amount)
                           });

            var mobileHeaderData = (from mo in filterStartDate == null ?
                                       Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                        x.SalesBy.Equals(salesId) &&
                                                                        x.Mark != "REJ")
                                     : Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                        x.SalesBy.Equals(salesId) &&
                                                                        x.Mark != "REJ" &&
                                                                        x.Date >= filterStartDate &&
                                                                        x.Date <= filterEndDate)
                                    join mod in Db.MobileOrderDetails on mo.Code equals mod.Code
                                    join it in filterGroupId != 0 ?
                                      Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                         (x.SubGroup1.Contains(sub.Value) ||
                                                          x.SubGroup2.Contains(sub.Value) ||
                                                          x.SubGroup3.Contains(sub.Value) ||
                                                          x.SubGroup4.Contains(sub.Value) ||
                                                          x.SubGroup5.Contains(sub.Value)))
                                    : Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) ||
                                                          x.SubGroup2.Contains(sub.Value) ||
                                                          x.SubGroup3.Contains(sub.Value) ||
                                                          x.SubGroup4.Contains(sub.Value) ||
                                                          x.SubGroup5.Contains(sub.Value)) on mod.ItemId equals it.Id
                                    join u in Db.UoMConversions on mod.UnitId equals u.Id
                                    select new
                                    {
                                        SalesId = mo.SalesBy,
                                        TotalNettPrice = mod.NettPrice * mod.Qty,
                                    });

            var mobileData = (from mo in mobileHeaderData
                              group new
                              {
                                  mo,
                              } by new { mo.SalesId } into g
                              select new TransactionHistoryDetailBySubGroupSummary
                              {
                                  SalesId = g.Key.SalesId,
                                  DetailSubGroup = sub.Value,
                                  Total = g.Sum(tl => tl.mo.TotalNettPrice),
                                  Target = targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault() == null ? 0 : targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault().Amount,
                                  PercentAchieved = targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault() == null ? 0 : g.Sum(tl => tl.mo.TotalNettPrice) / targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault().Amount * 100
                              }).ToList();

            var salesHeaderData = (from si in filterStartDate == null ?
                                     Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark))
                                   : Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark) &&
                                                                         x.Date >= filterStartDate &&
                                                                         x.Date <= filterEndDate)
                                   join sid in Db.SalesInvoiceDetails on si.Code equals sid.Code
                                   join sd in Db.VwSalesDeliveryHeaders on sid.DoCode equals sd.Code
                                   join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
                                   join so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId)) on si.SoCode equals so.Code
                                   join it in filterGroupId != 0 ?
                                     Db.Items.Where(x => x.SubGroup1.Contains(sub.Value) ||
                                                         x.SubGroup2.Contains(sub.Value) ||
                                                         x.SubGroup3.Contains(sub.Value) ||
                                                         x.SubGroup4.Contains(sub.Value) ||
                                                         x.SubGroup5.Contains(sub.Value))
                                   : Db.Items.Where(x => categories.Contains(x.CategoryId) &&
                                                        (x.SubGroup1.Contains(sub.Value) ||
                                                         x.SubGroup2.Contains(sub.Value) ||
                                                         x.SubGroup3.Contains(sub.Value) ||
                                                         x.SubGroup4.Contains(sub.Value) ||
                                                         x.SubGroup5.Contains(sub.Value))) on sdd.ItemId equals it.Id
                                   join u in Db.UoMConversions on sdd.UnitId equals u.Id
                                   select new
                                   {
                                       SalesId = so.SalesBy,
                                       TotalNettPrice = sdd.NettPrice * sdd.Qty,
                                   });

            var salesData = (from so in salesHeaderData
                             group new
                             {
                                 so,
                             } by new { so.SalesId } into g
                             select new TransactionHistoryDetailBySubGroupSummary
                             {
                                 SalesId = g.Key.SalesId,
                                 DetailSubGroup = sub.Value,
                                 Total = g.Sum(tl => tl.so.TotalNettPrice),
                                 Target = targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault() == null ? 0 : targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault().Amount,
                                 PercentAchieved = targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault() == null ? 0 : g.Sum(tl => tl.so.TotalNettPrice) / targets.Where(x => x.SalesId.Equals(g.Key.SalesId)).FirstOrDefault().Amount * 100
                             }).ToList();

            var dataUnion = salesData.Union(mobileData);

            var dataResult = (from du in dataUnion
                              group du by new
                              {
                                  du.SalesId,
                                  du.DetailSubGroup,
                                  du.Target
                              }
                              into g
                              select new TransactionHistoryDetailBySubGroupSummary
                              {
                                  SalesId = g.Key.SalesId,
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
        //string[] SOMarkIn = new string[] { "A", "PS", "CMP", ""CLS" }; // Active, Partial Shipment, Complete, Close
        //string[] SOMarkNotIn = new string[] { "V", "OL" }; // Void, Over Limit
        string[] SIMarkIn = new string[] { "A", "PP", "CMP" }; // Active, Partial Payment

        var mobileHeaderData = (from mo in filterStartDate == null ?
                                  Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                     x.SalesBy.Equals(salesId) &&
                                                                     x.Mark != "REJ")
                                : Db.VwMobileOrderHeaders.Where(x => x.SalesOrderCode.Equals(null) &&
                                                                     x.SalesBy.Equals(salesId) &&
                                                                     x.Mark != "REJ" &&
                                                                     x.Date >= filterStartDate &&
                                                                     x.Date <= filterEndDate)
                                join mod in Db.VwMobileOrderDetails on mo.Code equals mod.Code
                                join it in Db.Items.Where(x => x.SubGroup1.Contains(detailSubGroup) ||
                                                               x.SubGroup2.Contains(detailSubGroup) ||
                                                               x.SubGroup3.Contains(detailSubGroup) ||
                                                               x.SubGroup4.Contains(detailSubGroup) ||
                                                               x.SubGroup5.Contains(detailSubGroup)) on mod.ItemId equals it.Id
                                join u in Db.UoMConversions on mod.UnitId equals u.Id
                                select new
                                {
                                    SalesId = mo.SalesBy,
                                    ItemId = it.Id,
                                    ItemName = it.Name,
                                    u.UnitEquivalent,
                                    mod.Qty,
                                    mod.UnitId,
                                    Disc = mod.Disc,
                                    mod.FinalDiscHeader,
                                    mod.TaxAmount,
                                    mod.ExemptTaxAmount,
                                    TotalTaxAmount = mod.TaxAmount * mod.Qty,
                                    TotalExemptTaxAmount = mod.ExemptTaxAmount * mod.Qty,
                                    TotalNettPrice = mod.NettPrice * mod.Qty,
                                });

        var mobileData = (from mo in mobileHeaderData
                          group new
                          {
                              mo,
                          } by new
                          {
                              mo.SalesId,
                              mo.ItemId,
                              mo.ItemName,
                              mo.UnitId,
                              mo.UnitEquivalent
                          } into g
                          select new TransactionHistoryItemBySubGroupSummary
                          {
                              SalesId = g.Key.SalesId,
                              ItemId = g.Key.ItemId,
                              ItemName = g.Key.ItemName,
                              Unit = g.Key.UnitEquivalent,
                              Quantity = g.Sum(tl => tl.mo.Qty),
                              Discount = g.Sum(dc => dc.mo.Disc + dc.mo.FinalDiscHeader),
                              TaxAmount = g.Sum(tx => tx.mo.TaxAmount),
                              ExemptTaxAmount = g.Sum(etx => etx.mo.ExemptTaxAmount),
                              TotalReal = (decimal)0.00,
                              TotalMobile = g.Sum(etx => etx.mo.TotalNettPrice),
                              Total = g.Sum(tl => tl.mo.TotalNettPrice)
                          }).ToList();

        var salesHeaderData = (from si in filterStartDate == null ?
                                      Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark))
                                    : Db.VwSalesInvoiceHeaders.Where(x => SIMarkIn.Contains(x.Mark) &&
                                                                          x.Date >= filterStartDate &&
                                                                          x.Date <= filterEndDate)
                               join sid in Db.SalesInvoiceDetails on si.Code equals sid.Code
                               join sd in Db.VwSalesDeliveryHeaders on sid.DoCode equals sd.Code
                               join sdd in Db.VwSalesDeliveryDetails on sd.Code equals sdd.Code
                               join so in Db.VwSalesOrderHeaders.Where(x => x.SalesBy.Equals(salesId)) on si.SoCode equals so.Code
                               join it in Db.Items.Where(x => x.SubGroup1.Contains(detailSubGroup) ||
                                                              x.SubGroup2.Contains(detailSubGroup) ||
                                                              x.SubGroup3.Contains(detailSubGroup) ||
                                                              x.SubGroup4.Contains(detailSubGroup) ||
                                                              x.SubGroup5.Contains(detailSubGroup)) on sdd.ItemId equals it.Id
                               join u in Db.UoMConversions on sdd.UnitId equals u.Id
                               select new
                               {
                                   SalesId = so.SalesBy,
                                   ItemId = it.Id,
                                   ItemName = it.Name,
                                   u.UnitEquivalent,
                                   sdd.Qty,
                                   sdd.UnitId,
                                   Disc = sdd.Disc,
                                   sdd.FinalDiscHeader,
                                   sdd.TaxAmount,
                                   sdd.ExemptTaxAmount,
                                   TotalTaxAmount = sdd.TaxAmount * sdd.Qty,
                                   TotalExemptTaxAmount = sdd.ExemptTaxAmount * sdd.Qty,
                                   TotalNettPrice = sdd.NettPrice * sdd.Qty,
                               });

        var salesData = (from so in salesHeaderData
                         group new
                         {
                             so
                         } by new
                         {
                             so.SalesId,
                             so.ItemId,
                             so.ItemName,
                             so.UnitId,
                             so.UnitEquivalent
                         }
                         into g
                         select new TransactionHistoryItemBySubGroupSummary
                         {
                             SalesId = g.Key.SalesId,
                             ItemId = g.Key.ItemId,
                             ItemName = g.Key.ItemName,
                             Unit = g.Key.UnitEquivalent,
                             Quantity = g.Sum(qt => qt.so.Qty),
                             Discount = g.Sum(dc => dc.so.Disc + dc.so.FinalDiscHeader),
                             TaxAmount = g.Sum(tx => tx.so.TaxAmount),
                             ExemptTaxAmount = g.Sum(etx => etx.so.ExemptTaxAmount),
                             TotalReal = g.Sum(etx => etx.so.TotalNettPrice),
                             TotalMobile = (decimal)0.00,
                             Total = g.Sum(tl => tl.so.TotalNettPrice)
                         }).ToList();

        var dataUnion = salesData.Union(mobileData);

        var dataResult = (from so in dataUnion
                          group so by new
                          {
                              so.SalesId,
                              so.ItemId,
                              so.ItemName,
                              so.Unit
                          } into g
                          select new TransactionHistoryItemBySubGroupSummary
                          {
                              SalesId = g.Key.SalesId,
                              ItemId = g.Key.ItemId,
                              ItemName = g.Key.ItemName,
                              Unit = g.Key.Unit,
                              Quantity = g.Sum(tl => tl.Quantity),
                              Discount = g.Sum(dc => dc.Discount),
                              TaxAmount = g.Sum(tx => tx.TaxAmount),
                              ExemptTaxAmount = g.Sum(etx => etx.ExemptTaxAmount),
                              TotalReal = g.Sum(etx => etx.TotalReal), // Total Penjualan (Riil)
                              TotalMobile = g.Sum(etx => etx.TotalMobile), // Total Mobile
                              Total = g.Sum(etx => etx.TotalReal) + g.Sum(etx => etx.TotalMobile)
                          }).AsQueryable();

        DataSourceResult result = dataResult.ToDataSourceResult(skip, take, filter, sort);

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

    public DataSourceResult GetCustomerDetailByUnitProduct(int filterUnit, int itemId, DateTime? startDate, DateTime? endDate, int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).Single();
        var filterDate = "";
        var formatStartDate = startDate.HasValue ? startDate.ToString() : "n/a";
        var formatEndDate = endDate.HasValue ? endDate.ToString() : "n/a";
        var destSeq = 0;
        var destUnitName = "";

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date Between '{formatStartDate}' AND '{formatEndDate}' ";
        }
        else if (startDate != null && startDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date>='{formatStartDate}' ";
        }
        else if (endDate != null && endDate.HasValue)
        {
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date<='{formatEndDate}' ";
        }

        var history = Db.TransactionHistoryDetailCustomerByProductUnits.FromSqlRaw(
                            @"SELECT CustCode,CustName,ItemId,ItemName,UnitId,Seq,UnitName, SUM(Qty) as Qty,SUM(Total) as Total,UomId,
                                UnitBuyId,UnitBuyName,UnitBuySeq, UnitSellId,UnitSellName,UnitSellSeq,
                                UnitMaxId, UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
                                UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy 
                            FROM (
                                SELECT CustCode,C.Name as CustName,ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
                                    UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
                                    UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
                                    UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
                                    UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
                                    UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq,
                                    UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
                                FROM [MobileSales].[MobileOrderHeader] H 
                                    JOIN [MobileSales].[MobileOrderDetail] D on H.Code=D.Code
                                    JOIN [Inventory].[Item] I on D.ItemId=I.Id
                                    JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
                                    JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
                                    JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
                                    JOIN (
                                        SELECT id,UomId,UnitEquivalent,Seq
                                        FROM [Inventory].[UoMConversion]
                                        WHERE IsBaseUnit=1
                                    ) UBase on D.UomId=UBase.UomId
                                    JOIN (
                                        SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                        WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                    ) UMax on D.UomId=UMax.UomId
                                    JOIN (SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                        WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                    ) UMin on D.UomId=UMin.UomId
                                    JOIN (
                                        SELECT Code,Name FROM [General].[Customer]
                                        UNION ALL 
                                        SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
                                    ) C on H.CustCode=C.Code
                                WHERE SalesOrderCode is null AND Mark!='REJ' " + filterDate +
                                @"GROUP BY CustCode,C.Name,ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
                                    UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
                                    UMax.Id,UMax.UnitEquivalent,UMax.Seq,
                                    UMin.Id,UMin.UnitEquivalent,UMin.Seq,
                                    UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
                                UNION ALL
                                SELECT CustCode,C.Name as CustName,ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
                                    UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
                                    UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
                                    UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
                                    UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
                                    UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq,
                                    UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
                                FROM [Sales].[SalesOrderHeader] H 
                                    JOIN [Sales].[SalesOrderDetail] D on H.Code=D.Code 
                                    JOIN [Inventory].[Item] I on D.ItemId=I.Id
                                    JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
                                    JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
                                    JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
                                    JOIN (
                                        SELECT id,UomId,UnitEquivalent,Seq
                                        FROM [Inventory].[UoMConversion]
                                        WHERE IsBaseUnit=1
                                    ) UBase on D.UomId=UBase.UomId
                                    JOIN (
                                        SELECT id,uomid,UnitEquivalent,Seq
                                        FROM [Inventory].[UoMConversion] U
                                        WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                    ) UMax on D.UomId=UMax.UomId
                                    JOIN (
                                        SELECT id,uomid,UnitEquivalent,Seq
                                        FROM [Inventory].[UoMConversion] U
                                        WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                    ) UMin on D.UomId=UMin.UomId
                                    JOIN (
                                        SELECT Code,Name FROM [General].[Customer]
                                        UNION ALL
                                        SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
                                    ) C on H.CustCode=C.Code
                                WHERE (Mark='A' OR Mark='PS' OR Mark='CMP') OR (Mark='CLS' AND QtyDlv>0) " + filterDate +
                                @"GROUP BY CustCode,C.Name,ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
                                    UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
                                    UMax.Id,UMax.UnitEquivalent,UMax.Seq,
                                    UMin.Id,UMin.UnitEquivalent,UMin.Seq,
                                    UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
                            ) A 
                            GROUP BY CustCode,CustName,ItemId,ItemName,UnitId,UnitName,Seq, UomId, UnitBuyId,UnitBuyName,UnitBuySeq,
                                UnitSellId,UnitSellName,UnitSellSeq,
                                UnitMaxId,UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
                                UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy
                            ").Where(x => x.SalesBy.Equals(salesId) && x.ItemId.Equals(itemId)).ToList();

        List<TransactionCustomerDetailByUnit> resultData = new();
        foreach (var data in history)
        {
            switch (filterUnit)
            {
                case 1: //base
                    destSeq = data.UnitBaseSeq;
                    destUnitName = data.UnitBaseName;
                    break;
                case 2: //biggest
                    destSeq = data.UnitMaxSeq;
                    destUnitName = data.UnitMaxName;
                    break;
                case 3: //smallest
                    destSeq = data.UnitMinSeq;
                    destUnitName = data.UnitMinName;
                    break;
                case 4: //buy
                    destSeq = data.UnitBuySeq;
                    destUnitName = data.UnitBuyName;
                    break;
                case 5: //sell
                    destSeq = data.UnitSellSeq;
                    destUnitName = data.UnitSellName;
                    break;
            }

            if (data.Seq > destSeq)
            {
                var qtyConverted = CalculateUnitToBiggerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    CustCode = data.CustCode,
                    CustName = data.CustName,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty * qtyConverted,
                    Total = data.Total

                });
            }
            else if (data.Seq < destSeq)
            {
                var qtyConverted = CalculateUnitToSmallerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    CustCode = data.CustCode,
                    CustName = data.CustName,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty / qtyConverted,
                    Total = data.Total
                });
            }
            else
            {
                resultData.Add(new()
                {
                    CustCode = data.CustCode,
                    CustName = data.CustName,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty,
                    Total = data.Total
                });
            }
        }

        var resultSum = from r in resultData
                        group r by new { r.CustCode, r.CustName, r.ItemId, r.ItemName, r.Unit } into g
                        select new
                        {
                            CustCode = g.Key.CustCode,
                            CustName = g.Key.CustName,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Unit = g.Key.Unit,
                            Quantity = g.Sum(tl => tl.Quantity),
                            Total = g.Sum(tl => tl.Total),
                        };


        DataSourceResult result = resultSum.AsQueryable().ToDataSourceResult(skip, take, filter, sort);

        return result;

    }

    public DataSourceResult GetItemDetailByUnitProduct(int filterUnit, int itemId, string custCode, DateTime? startDate, DateTime? endDate, int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        var filterDate = "";
        var formatStartDate = startDate.HasValue ? startDate.ToString() : "n/a";
        var formatEndDate = endDate.HasValue ? endDate.ToString() : "n/a";
        var destSeq = 0;
        var destUnitName = "";

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date Between '{formatStartDate}' AND '{formatEndDate}' ";
        }
        else if (startDate != null && startDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date>='{formatStartDate}' ";
        }
        else if (endDate != null && endDate.HasValue)
        {
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date<='{formatEndDate}' ";
        }

        var history = Db.TransactionHistoryDetailItemByProductUnits.FromSqlRaw(
                        @"SELECT TransCode,Date,CustCode,CustName,ItemId,ItemName,UnitId,Seq,UnitName, SUM(Qty) as Qty,SUM(Total) as Total,UomId,
                            UnitBuyId,UnitBuyName,UnitBuySeq, UnitSellId,UnitSellName,UnitSellSeq,
                            UnitMaxId, UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
                            UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy
                        FROM (
                            SELECT H.Code as TransCode,Date,CustCode,C.Name as CustName,ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
                                UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
                                UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
                                UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
                                UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
                                UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq,
                                UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
                            FROM [MobileSales].[MobileOrderHeader] H
                                JOIN [MobileSales].[MobileOrderDetail] D on H.Code=D.Code
                                JOIN [Inventory].[Item] I on D.ItemId=I.Id
                                JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
                                JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
                                JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
                                JOIN (
                                    SELECT id,UomId,UnitEquivalent,Seq
                                    FROM [Inventory].[UoMConversion]
                                    WHERE IsBaseUnit=1
                                    ) UBase on D.UomId=UBase.UomId
                                JOIN (
                                    SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                    WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                ) UMax on D.UomId=UMax.UomId
                                JOIN (
                                    SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                    WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                ) UMin on D.UomId=UMin.UomId
                                JOIN (
                                    SELECT Code,Name FROM [General].[Customer]
                                    UNION ALL
                                    SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
                                ) C on H.CustCode=C.Code
                            WHERE SalesOrderCode is null AND Mark!='REJ' " + filterDate +
                            @"GROUP BY H.Code,Date,CustCode,C.Name,ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
                                UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
                                UMax.Id,UMax.UnitEquivalent,UMax.Seq,
                                UMin.Id,UMin.UnitEquivalent,UMin.Seq,
                                UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
                            UNION ALL
                            SELECT H.Code as TransCode,Date,CustCode,C.Name as CustName,ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
                                UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
                                UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
                                UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
                                UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
                                UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq,
                                UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
                            FROM [Sales].[SalesOrderHeader] H 
                                JOIN [Sales].[SalesOrderDetail] D on H.Code=D.Code 
                                JOIN [Inventory].[Item] I on D.ItemId=I.Id
                                JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
                                JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
                                JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
                                JOIN (
                                    SELECT id,UomId,UnitEquivalent,Seq
                                    FROM [Inventory].[UoMConversion]
                                    WHERE IsBaseUnit=1
                                ) UBase on D.UomId=UBase.UomId
                                JOIN (
                                    SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                    WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                ) UMax on D.UomId=UMax.UomId
                                JOIN (
                                    SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
                                    WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                                ) UMin on D.UomId=UMin.UomId
                                JOIN (
                                    SELECT Code,Name FROM [General].[Customer]
                                    UNION ALL
                                    SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
                                ) C on H.CustCode=C.Code
                            WHERE (Mark='A' OR Mark='PS' OR Mark='CMP') OR (Mark='CLS' AND QtyDlv>0) " + filterDate +
                            @"GROUP BY H.Code,Date,CustCode,C.Name,ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
                                UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
                                UMax.Id,UMax.UnitEquivalent,UMax.Seq,
                                UMin.Id,UMin.UnitEquivalent,UMin.Seq,
                                UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
                        ) A
                        GROUP BY TransCode,Date,CustCode,CustName,ItemId,ItemName,UnitId,UnitName,Seq, UomId, UnitBuyId,UnitBuyName,UnitBuySeq,
                            UnitSellId,UnitSellName,UnitSellSeq,
                            UnitMaxId,UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
                            UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy
                        ").Where(x => x.SalesBy.Equals(salesId) && x.ItemId.Equals(itemId) && x.CustCode.Equals(custCode)).ToList();

        List<TransactionItemDetailByUnit> resultData = new();
        foreach (var data in history)
        {
            switch (filterUnit)
            {
                case 1: //base
                    destSeq = data.UnitBaseSeq;
                    destUnitName = data.UnitBaseName;
                    break;
                case 2: //biggest
                    destSeq = data.UnitMaxSeq;
                    destUnitName = data.UnitMaxName;
                    break;
                case 3: //smallest
                    destSeq = data.UnitMinSeq;
                    destUnitName = data.UnitMinName;
                    break;
                case 4: //buy
                    destSeq = data.UnitBuySeq;
                    destUnitName = data.UnitBuyName;
                    break;
                case 5: //sell
                    destSeq = data.UnitSellSeq;
                    destUnitName = data.UnitSellName;
                    break;
            }

            if (data.Seq > destSeq)
            {
                var qtyConverted = CalculateUnitToBiggerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    TransCode = data.TransCode,
                    Date = data.Date,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty * qtyConverted,
                    Total = data.Total
                });
            }
            else if (data.Seq < destSeq)
            {
                var qtyConverted = CalculateUnitToSmallerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    TransCode = data.TransCode,
                    Date = data.Date,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty / qtyConverted,
                    Total = data.Total
                });
            }
            else
            {
                resultData.Add(new()
                {
                    TransCode = data.TransCode,
                    Date = data.Date,
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty,
                    Total = data.Total
                });
            }
        }

        var resultSum = from r in resultData
                        group r by new { r.TransCode, r.Date, r.ItemId, r.ItemName, r.Unit } into g
                        select new
                        {
                            TransCode = g.Key.TransCode,
                            Date = g.Key.Date,
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Unit = g.Key.Unit,
                            Quantity = g.Sum(tl => tl.Quantity),
                            Total = g.Sum(tl => tl.Total),
                        };


        DataSourceResult result = resultSum.AsQueryable().ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    public DataSourceResult GetItemByUnitProduct(int filterUnit, DateTime? startDate, DateTime? endDate, int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId)
    {
        var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        var filterDate = "";
        var formatStartDate = startDate.HasValue ? startDate.ToString() : "n/a";
        var formatEndDate = endDate.HasValue ? endDate.ToString() : "n/a";
        var destSeq = 0;
        var destUnitName = "";

        if (startDate != null && startDate.HasValue && endDate != null && endDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date Between '{formatStartDate}' AND '{formatEndDate}' ";
        }
        else if (startDate != null && startDate.HasValue)
        {
            formatStartDate = Convert.ToDateTime(formatStartDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date>='{formatStartDate}' ";
        }
        else if (endDate != null && endDate.HasValue)
        {
            formatEndDate = Convert.ToDateTime(formatEndDate).ToString("yyyy-MM-dd");
            filterDate = $" AND Date<='{formatEndDate}' ";
        }

        var history = Db.TransactionHistoryByProductUnits.FromSqlRaw(
                        @"SELECT ItemId,ItemName,UnitId,Seq,UnitName, SUM(Qty) as Qty,SUM(Total) as Total,UomId,
                            UnitBuyId,UnitBuyName,UnitBuySeq, UnitSellId,UnitSellName,UnitSellSeq,
                            UnitMaxId, UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
	                        UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy
                        FROM (
	                        SELECT ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
		                        UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
		                        UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
		                        UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
		                        UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
		                        UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq,
		                        UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
	                        FROM [MobileSales].[MobileOrderHeader] H
	                        JOIN [MobileSales].[MobileOrderDetail] D on H.Code=D.Code
	                        JOIN [Inventory].[Item] I on D.ItemId=I.Id
	                        JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
	                        JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
	                        JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
	                        JOIN (
		                        SELECT id,UomId,UnitEquivalent,Seq
		                        FROM [Inventory].[UoMConversion]
		                        WHERE IsBaseUnit=1
	                        ) UBase on D.UomId=UBase.UomId
	                        JOIN (
		                        SELECT id,uomid,UnitEquivalent,Seq
		                        FROM [Inventory].[UoMConversion] U
		                        WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
	                        ) UMax on D.UomId=UMax.UomId
	                        JOIN (
		                        SELECT id,uomid,UnitEquivalent,Seq
		                        FROM [Inventory].[UoMConversion] U
		                        WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
	                        ) UMin on D.UomId=UMin.UomId
	                        JOIN (
                                SELECT Code,Name FROM [General].[Customer]
		                        UNION ALL
		                        SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
	                        ) C on H.CustCode=C.Code
	                        WHERE SalesOrderCode is null AND Mark!='REJ' " + filterDate +
                            @"GROUP BY ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
		                        UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
		                        UMax.Id,UMax.UnitEquivalent,UMax.Seq,
		                        UMin.Id,UMin.UnitEquivalent,UMin.Seq,
		                        UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
	                        UNION ALL	
	                        SELECT ItemId,I.Name as ItemName,D.UnitId,UReal.UnitEquivalent as UnitName,
		                        UReal.Seq,SUM(Qty) as Qty,SUM(D.NettPrice*D.Qty) as Total,I.UomId,
		                        UomBuyId as UnitBuyId,UBuy.UnitEquivalent as UnitBuyName, UBuy.Seq as UnitBuySeq,
		                        UomSellId as UnitSellId,USell.UnitEquivalent as UnitSellName,USell.Seq as UnitSellSeq,
		                        UMax.Id as UnitMaxId,UMax.UnitEquivalent as UnitMaxName, UMax.Seq as UnitMaxSeq,
		                        UMin.Id as UnitMinId,UMin.UnitEquivalent as UnitMinName,UMin.Seq as UnitMinSeq, 
		                        UBase.Id as UnitBaseId,UBase.UnitEquivalent as UnitBaseName,UBase.Seq as UnitBaseSeq,SalesBy
	                        FROM [Sales].[SalesOrderHeader] H
	                        JOIN [Sales].[SalesOrderDetail] D on H.Code=D.Code
	                        JOIN [Inventory].[Item] I on D.ItemId=I.Id
	                        JOIN [Inventory].[UoMConversion] UReal on D.UnitId=UReal.Id
	                        JOIN [Inventory].[UoMConversion] UBuy on I.UomBuyId=UBuy.Id
	                        JOIN [Inventory].[UoMConversion] USell on I.UomSellId=USell.Id
	                        JOIN (
                                SELECT id,UomId,UnitEquivalent,Seq
                                FROM [Inventory].[UoMConversion]
                                WHERE IsBaseUnit=1
                            ) UBase on D.UomId=UBase.UomId
	                        JOIN (
                                SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
		                        WHERE Seq=(Select max(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                            ) UMax on D.UomId=UMax.UomId
	                        JOIN (
                                SELECT id,uomid,UnitEquivalent,Seq FROM [Inventory].[UoMConversion] U
		                        WHERE Seq=(Select min(seq) FROM [Inventory].[UoMConversion] Where UomId=U.UomId group by UomId)
                            ) UMin on D.UomId=UMin.UomId
	                        JOIN (
                                SELECT Code,Name FROM [General].[Customer]
	                            UNION ALL
		                        SELECT Code,Name FROM [MobileSales].[MobileCustomer] WHERE Mark='A'
                            ) C on H.CustCode=C.Code
                            WHERE (Mark='A' OR Mark='PS' OR Mark='CMP') OR (Mark='CLS' AND QtyDlv>0) " + filterDate +
                            @"GROUP BY ItemId,I.Name,D.UnitId,UReal.UnitEquivalent,UReal.Seq,I.UomId,UomBuyId,UBuy.UnitEquivalent,
                                UBuy.Seq,UomSellId,USell.UnitEquivalent,USell.Seq,
                                UMax.Id,UMax.UnitEquivalent,UMax.Seq,
                                UMin.Id,UMin.UnitEquivalent,UMin.Seq,
                                UBase.Id,UBase.UnitEquivalent,UBase.Seq,SalesBy
                        ) A
                        GROUP BY ItemId,ItemName,UnitId,UnitName,Seq, UomId, UnitBuyId,UnitBuyName,UnitBuySeq,
	                        UnitSellId,UnitSellName,UnitSellSeq,
	                        UnitMaxId,UnitMaxName,UnitMaxSeq,UnitMinId,UnitMinName,UnitMinSeq,
	                        UnitBaseId,UnitBaseName,UnitBaseSeq,SalesBy"
                    ).Where(x => x.SalesBy.Equals(salesId)).ToList();

        List<TransactionHistoryByProductUnit> resultData = new();
        foreach (var data in history)
        {
            switch (filterUnit)
            {
                case 1: //base
                    destSeq = data.UnitBaseSeq;
                    destUnitName = data.UnitBaseName;
                    break;
                case 2: //biggest
                    destSeq = data.UnitMaxSeq;
                    destUnitName = data.UnitMaxName;
                    break;
                case 3: //smallest
                    destSeq = data.UnitMinSeq;
                    destUnitName = data.UnitMinName;
                    break;
                case 4: //buy
                    destSeq = data.UnitBuySeq;
                    destUnitName = data.UnitBuyName;
                    break;
                case 5: //sell
                    destSeq = data.UnitSellSeq;
                    destUnitName = data.UnitSellName;
                    break;
            }

            if (data.Seq > destSeq)
            {
                var qtyConverted = CalculateUnitToBiggerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty * qtyConverted,
                    Total = data.Total

                });
            }
            else if (data.Seq < destSeq)
            {
                var qtyConverted = CalculateUnitToSmallerSeq(data.UomId, data.Seq, destSeq);
                resultData.Add(new()
                {
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty / qtyConverted,
                    Total = data.Total
                });
            }
            else
            {
                resultData.Add(new()
                {
                    ItemId = data.ItemId,
                    ItemName = data.ItemName,
                    Unit = destUnitName,
                    Quantity = data.Qty,
                    Total = data.Total
                });
            }
        }

        var resultSum = from r in resultData
                        group r by new { r.ItemId, r.ItemName, r.Unit } into g
                        select new
                        {
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.ItemName,
                            Unit = g.Key.Unit,
                            Quantity = g.Sum(tl => tl.Quantity),
                            Total = g.Sum(tl => tl.Total),
                        };

        DataSourceResult result = resultSum.AsQueryable().ToDataSourceResult(skip, take, filter, sort);

        return result;
    }

    private decimal CalculateUnitToSmallerSeq(int uomId, int sourceSeq, int destSeq)
    {
        decimal result = 1;
        List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(uomId)).OrderBy(x => x.Seq).ToList();

        for (int i = conversions.Count - 1; i > 0; i--)
        {
            if (conversions[i].Seq <= (destSeq) && conversions[i].Seq > sourceSeq)
            {
                result *= conversions[i].Conversion;
            }
        }

        return result;
    }

    private decimal CalculateUnitToBiggerSeq(int uomId, int sourceSeq, int destSeq)
    {
        decimal result = 1;
        List<UoMConversion> conversions = Db.UoMConversions.Where(x => x.UomId.Equals(uomId)).OrderBy(x => x.Seq).ToList();

        for (int i = 0; i < conversions.Count; i++)
        {
            if (conversions[i].Seq >= (destSeq + 1) && conversions[i].Seq <= sourceSeq)
            {
                result *= conversions[i].Conversion;
            }
        }

        return result;
    }
}