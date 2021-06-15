using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemService : GeneralService<Item>, IItemService
    {
        public ItemService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category,string warehouseCode, string search)
        {

            var data = Db.VwItems.AsQueryable();

            if (category?.Any() ?? false)
            {
                data = data.Where(x => category.Contains(x.CategoryId));
            }

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
                wq = wq.Where(x=>x.WarehouseCode.Equals(warehouseCode))
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
            else {
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
                        //ValuationMethod = x.ValuationMethod,
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

            // Update data
            Db.Items.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

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
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Tidak bisa menonaktifkan data barang karena data sudah nonaktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data barang berhasil dinonaktifkan.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }
    }
}
