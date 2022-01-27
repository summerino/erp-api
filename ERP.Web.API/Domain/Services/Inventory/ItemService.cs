using System.Linq.Dynamic.Core;
using Microsoft.Data.SqlClient;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Services.Inventory
{
    public class ItemService : GeneralService<Item>, IItemService
    {
        public ItemService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string warehouseCode, string search, string mobileLastSync)
        {
            var data = Db.VwItems.AsQueryable();

            if (!string.IsNullOrEmpty(mobileLastSync))
            {
                switch (mobileLastSync.Length)
                {
                    case 21:
                        mobileLastSync += "000";
                        break;
                    case 22:
                        mobileLastSync += "00";
                        break;
                    case 23:
                        mobileLastSync += "0";
                        break;
                }
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(mobileLastSync, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            if (category?.Any() ?? false)
                data = data.Where(x => category.Contains(x.CategoryId));

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Name.Contains(search) ||
                            x.UomInitial.Contains(search) || x.UomSellName.Contains(search) ||
                            x.UomBuyName.Contains(search) || x.CategoryName.Contains(search));
            }

            IQueryable<WarehouseQuantity> wq = Db.WarehouseQuantities;


            if (!string.IsNullOrEmpty(warehouseCode))
            {
                wq = wq.Where(x => x.WarehouseCode.Equals(warehouseCode))
                        .GroupBy(x => new { x.ItemId, x.WarehouseCode })
                        .Select(X =>
                        new WarehouseQuantity
                        {
                            ItemId = X.Key.ItemId,
                            WarehouseCode = X.Key.WarehouseCode,
                            QtyOnHand = X.Sum(x => x.QtyOnHand),
                            QtyOnIndent = X.Sum(x => x.QtyOnIndent),
                            QtyOnOrder = X.Sum(x => x.QtyOnOrder),
                            QtyOnTransfer = X.Sum(x => x.QtyOnTransfer),
                            QtyReorderPoint = X.Sum(x => x.QtyReorderPoint)
                        }).AsQueryable();
            }
            else
            {
                wq = wq.GroupBy(x => new { x.ItemId })
                           .Select(X =>
                           new WarehouseQuantity
                           {
                               ItemId = X.Key.ItemId,
                               WarehouseCode = "-",
                               QtyOnHand = X.Sum(x => x.QtyOnHand),
                               QtyOnIndent = X.Sum(x => x.QtyOnIndent),
                               QtyOnOrder = X.Sum(x => x.QtyOnOrder),
                               QtyOnTransfer = X.Sum(x => x.QtyOnTransfer),
                               QtyReorderPoint = X.Sum(x => x.QtyReorderPoint)
                           }).AsQueryable();
            }



            data = (from x in data
                    join y in wq on x.Id equals y.ItemId into gd
                    from g in gd.DefaultIfEmpty()
                    select new VwItem
                    {
                        Id = x.Id,
                        Initial = x.Initial,
                        Name = x.Name,
                        Description = x.Description,
                        BuyPrice = x.BuyPrice,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        CategoryId = x.CategoryId,
                        CategoryName = x.CategoryName,
                        CoaCogs = x.CoaCogs,
                        CoaPurcDisc = x.CoaPurcDisc,
                        CoaPurc = x.CoaPurc,
                        CoaCost = x.CoaCost,
                        CoaExpense = x.CoaExpense,
                        CoaInventory = x.CoaInventory,
                        CoaOffSet = x.CoaOffSet,
                        CoaPurcReturn = x.CoaPurcReturn,
                        CoaSls = x.CoaSls,
                        CoaSlsDisc = x.CoaSlsDisc,
                        CoaSlsReturn = x.CoaSlsReturn,
                        CostOfGoodSold = x.CostOfGoodSold,
                        CreatedInitial = x.CreatedInitial,
                        DimensionMeasurement = x.DimensionMeasurement,
                        UpdatedDate = x.UpdatedDate,
                        UpdatedInitial = x.UpdatedInitial,
                        UpdatedBy = x.UpdatedBy,
                        Height = x.Height,
                        IsActive = x.IsActive,
                        Length = x.Length,
                        PurchaseTaxId = x.PurchaseTaxId,
                        SalesTaxId = x.SalesTaxId,
                        SellPrice = x.SellPrice,
                        StockType = x.StockType,
                        SubGroup1 = x.SubGroup1,
                        SubGroup2 = x.SubGroup2,
                        SubGroup3 = x.SubGroup3,
                        SubGroup4 = x.SubGroup4,
                        SubGroup5 = x.SubGroup5,
                        TypeId = x.TypeId,
                        TypeName = x.TypeName,
                        UomBuyId = x.UomBuyId,
                        UomBuyName = x.UomBuyName,
                        UomId = x.UomId,
                        UomInitial = x.UomInitial,
                        UomSellId = x.UomSellId,
                        UomSellName = x.UomSellName,
                        Weight = x.Weight,
                        WeightMeasurement = x.WeightMeasurement,
                        Width = x.Width,
                        BuySeq = x.BuySeq,
                        SellSeq = x.SellSeq,
                        WarehouseCode = g.WarehouseCode == null ? "" : g.WarehouseCode,
                        QtyOnHand = g.QtyOnHand == 0 ? 0 : g.QtyOnHand,
                        QtyOnIndent = g.QtyOnIndent == 0 ? 0 : g.QtyOnIndent,
                        QtyOnOrder = g.QtyOnOrder == 0 ? 0 : g.QtyOnOrder,
                        QtyOnTransfer = g.QtyOnTransfer == 0 ? 0 : g.QtyOnTransfer
                    });

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public override SaveResult Insert(Item data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                // Insert data
                Db.Items.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data barang berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(Item data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            var wqData = Db.WarehouseQuantities.Where(x => x.ItemId == data.Id);

            // Update data
            Db.Items.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;
            if (wqData.Any())
            {
                Db.Entry(data).Property(e => e.Initial).IsModified = false;
                Db.Entry(data).Property(e => e.UomId).IsModified = false;
            }
            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data barang berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Items.Find(id);
            if (data != null)
            {
                try
                {
                    Db.Items.Remove(data);
                    Db.SaveChanges();
                }
                catch (Exception e)
                {
                    var ex = e?.InnerException as SqlException;
                    if (ex?.Number != 547) throw;

                    result.Message = "Data tidak bisa dihapus karena sedang digunakan oleh data lain.";
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data barang berhasil dihapus.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }

        public IEnumerable<dynamic> GetRelatedOrderTrans(string whid, int itemid)
        {
            var stockM = Db.StockMutations.Where(x => x.WarehouseCode == whid && x.ItemId == itemid && x.Type == "OO").ToList();
            var header = Db.VwSalesOrderHeaders.Where(s => (new string[] { "A", "PS" }).Contains(s.Mark) && stockM.Select(x => x.RefCode1).Contains(s.Code)).ToList();
            var details = Db.VwSalesOrderDetails.Where(r => header.Select(x => x.Code).Contains(r.Code) && r.ItemId == itemid).ToList();
            var free = Db.SalesOrderDetailFreeGoods.Where(r => header.Select(x => x.Code).Contains(r.Code) && r.ItemId == itemid).ToList();
            var result = (
                        new[] { new { Code = "", Date = new DateTime(), Type = "", CustName = "", Qty = 0, QtyDlv = 0, QtyRemain = 0 } }
                        ).Union(from h in header
                                join d in details on h.Code equals d.Code
                                select new
                                {
                                    Code = h.Code,
                                    Date = h.Date,
                                    Type = h.FromDirectInvoice == true ? "Penjualan Langsung" : "Order Penjualan",
                                    CustName = h.CustName,
                                    Qty = Convert.ToInt32(d.Qty),
                                    QtyDlv = Convert.ToInt32(d.QtyDlv),
                                    QtyRemain = Convert.ToInt32(d.Qty - d.QtyDlv)
                                }).Union(from h in header
                                         join f in free on h.Code equals f.Code
                                         select new
                                         {
                                             Code = h.Code,
                                             Date = h.Date,
                                             Type = "Bonus",
                                             CustName = h.CustName,
                                             Qty = Convert.ToInt32(f.Qty),
                                             QtyDlv = Convert.ToInt32(f.QtyClosed),
                                             QtyRemain = Convert.ToInt32(f.Qty - f.QtyClosed)
                                         }).Skip(1);
            return result;
        }

        public IEnumerable<dynamic> GetRelatedIndentTrans(string whid, int itemid)
        {
            var stockM = Db.StockMutations.Where(x => x.WarehouseCode == whid && x.ItemId == itemid && x.Type == "OI").ToList();
            var header = Db.VwPurchaseOrderHeaders.Where(s => (new string[] { "A", "PR" }).Contains(s.Mark) && stockM.Select(x => x.RefCode1).Contains(s.Code)).ToList();
            var details = Db.VwPurchaseOrderDetails.Where(r => header.Select(x => x.Code).Contains(r.Code) && r.ItemId == itemid).ToList();
            var result = (from h in header
                          join d in details on h.Code equals d.Code
                          select new
                          {
                              Code = h.Code,
                              Date = h.Date,
                              Type = "Order Pembelian",
                              Qty = Convert.ToInt32(d.Qty),
                              QtyDlv = Convert.ToInt32(d.QtyRcv),
                              QtyRemain = Convert.ToInt32(d.Qty - d.QtyRcv)
                          }).ToDynamicList();

            return result;

        }

        public IEnumerable<dynamic> GetRelatedTransferTrans(string whid, int itemid)
        {
            var stockM = Db.StockMutations.Where(x => x.WarehouseCode == whid && x.ItemId == itemid && x.Type == "OT").ToList();
            var header = Db.VwTransferStockHeaders.Where(s => s.Mark == "A" && s.Type == "OUT" && stockM.Select(x => x.RefCode1).Contains(s.Code)).ToList();
            var details = Db.VwTransferStockDetails.Where(r => header.Select(x => x.Code).Contains(r.Code) && r.ItemId == itemid).ToList();
            var result = (from h in header
                          join d in details on h.Code equals d.Code
                          select new
                          {
                              Code = h.Code,
                              Date = h.Date,
                              Type = "Transfer Persediaan",
                              Qty = Convert.ToInt32(d.Qty)
                          }).ToDynamicList();

            return result;
        }

        public bool IsItemUsed(int id)
        {
            return Db.WarehouseQuantities.Where(x => x.ItemId == id).Any();
        }

        #region Mobile
        public ItemInformationModel GetItemInformation(int itemId, string custCode)
        {
            var dataStock = Db.WarehouseQuantities.Where(x => x.ItemId.Equals(itemId));

            var stock = dataStock.Sum(p => p.QtyOnHand - p.QtyOnOrder);

            var uomId = Db.Items.Where(x=>x.Id.Equals(itemId)).Select(y=>y.UomId).First();

            var conv= new List<UoMConversion>();
            if (uomId.HasValue)
            {
                conv = Db.UoMConversions.Where(y => y.UomId.Equals(uomId)).OrderBy(x => x.Seq).ToList();
            }

            for (var i = 0; i < conv.Count; i++)
            {
                stock /= conv[i].Conversion;
            }

            DateTime? lastUpdate = null;
            if (dataStock.Any())
            {
                lastUpdate = dataStock.Max(p => p.UpdatedDate);
            }


            var data = (from sh in Db.SalesOrderHeaders
                        join sd in Db.SalesOrderDetails on sh.Code equals sd.Code
                        join u in Db.UoMConversions on sd.UnitId equals u.Id
                        where sd.ItemId == itemId && sh.CustCode == custCode
                        select new ItemInformtionDetailModel
                        {
                            Seq = u.Seq,
                            UomId = sd.UomId,
                            UnitId = sd.UnitId,
                            Qty = sd.Qty,
                            UpdatedDate = sh.UpdatedDate
                        }).ToList();

            if (data.Any())
            {

                foreach (var item in data)
                {
                    if (item.Seq != conv.Count)
                    {
                        for (var i = item.Seq; i < conv.Count; i++)
                        {
                            item.Qty /= conv[i].Conversion;
                        }
                    }
                }

                DateTime? maxUpdate = data.Max(p => p.UpdatedDate);
                var maxOrder = data.Max(p => p.Qty);
                var avgOrder = data.Average(p => p.Qty);
                var lastOrder = data.Where(x => x.UpdatedDate.Equals(maxUpdate)).Select(x => x.Qty).FirstOrDefault();

                return new ItemInformationModel
                {
                    UnitName = conv[conv.Count-1].UnitEquivalent,
                    Stock = stock,
                    LastUpdateStock = lastUpdate,
                    MaxOrder = maxOrder,
                    AvgOrder = avgOrder,
                    LastOrder = lastOrder
                };
            }
            else
            {
                return new ItemInformationModel
                {
                    UnitName = conv[conv.Count - 1].UnitEquivalent,
                    Stock = stock,
                    MaxOrder = 0,
                    AvgOrder = 0,
                    LastOrder = 0
                };
            }
        }
        #endregion
    }
}
