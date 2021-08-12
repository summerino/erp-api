using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Services.Inventory
{
    public class BeginningBalanceStockService : GeneralService<BeginningBalanceStockHeader>, IBeginningBalanceStockService
    {
        public BeginningBalanceStockService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwBeginningBalanceStockHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.WarehouseInitial.Contains(search) || x.Notes.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwBeginningBalanceStockDetail> GetDetailData(string code)
        {
            var data = Db.VwBeginningBalanceStockDetails.Where(x => x.Code == code);
            return data.OrderBy(x => x.LineNo);
        }
        public DataSourceResult GetBeginningBalanceStockItem(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
          List<int> category, string search)
        {
            var data = Db.VwBeginningBalanceItems.AsQueryable();

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

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult Insert(BeginningBalanceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("ADJ_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.BeginningBalanceStockHeaders.Add(data);

                // Insert detail data - different unit in a batch
                short i = 0;
                var details = new List<BeginningBalanceStockDetail>();
                foreach (var item in data.ItemDetails)
                {
                    var beginningBalanceDetail = new BeginningBalanceStockDetail() {
                        LineNo = ++i,
                        Code = newCode,
                        ItemId = item.ItemId,
                        Notes = item.Notes,
                        Qty = item.Qty,
                        Amount = item.Amount,
                        UnitId = item.UnitId,
                        UomId = item.UomId
                    };

                    details.Add(beginningBalanceDetail);
                }
                if (details.Any()) {
                    Db.BeginningBalanceStockDetails.AddRange(details);
                    Db.SaveChanges();
                }
                
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_bb
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_bb {0}, {1}",
                    data.Code, data.Date);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data saldo awal berhasil disimpan.";
            return result;
        }

        public SaveResult Update(BeginningBalanceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {

                // Checking mark header data
                //if (Db.BeginningBalanceStockHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                //{
                //    result.Message = "Data saldo awal tidak bisa di ubah karena sudah ditandai sebagai void.";
                //    return result;
                //}

                //data.ApprovedBy = null;
                //data.ApprovedDate = null;

                // Update header data
                Db.BeginningBalanceStockHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                //Get detail data that exists in order before
                var delDetails = Db.BeginningBalanceStockDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.BeginningBalanceStockDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {

                    if (item.Id <= 0)
                    {
                        var beginningBalanceDetail = new BeginningBalanceStockDetail()
                        {
                            Code = data.Code,
                            ItemId = item.ItemId,
                            LineNo = ++i,
                            Notes = item.Notes,
                            Qty = item.Qty,
                            Amount = item.Amount,
                            UnitId = item.UnitId,
                            UomId = item.UomId,
                        };
                        Db.BeginningBalanceStockDetails.Add(beginningBalanceDetail);
                    }
                    else
                    {
                        item.LineNo = ++i;
                        Db.BeginningBalanceStockDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_bb
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_bb {0}, {1}",
                    data.Code, data.Date);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data saldo awal berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var data = Db.BeginningBalanceStockHeaders.Find(code);
                if (data != null)
                {

                    // Execute sp_update_stock_mutation_from_bb
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_restore_stock_mutation_from_bb {0}, {1}",
                        data.Code, data.Date);

                    // Checking mark header data
                    //if (data.Mark == "V")
                    //{
                    //    result.Message = "Data penyesuaian tidak bisa di ubah karena sudah ditandai sebagai void.";
                    //    return result;
                    //}

                    // Update header data
                    // data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    Db.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
            

            result.Success = true;
            result.Message = "Data saldo awal berhasil ditandai sebagai void.";
            return result;
        }

        private StockMutation GetStockMutation(BeginningBalanceRequest data, BeginningBalanceStockDetail item)
        {
            return new StockMutation
            {
                WarehouseCode = data.WarehouseCode,
                Date = data.Date,
                ItemId = item.ItemId,
                Qty = item.Qty,
                RefCode1 = data.Code,
                RefDetailId1 = item.Id,
                Src = "BB",
                RefCode2 = null,
                UnitId = item.UnitId,
                UomId = item.UomId,
                Type = "OH",
                BaseQty = item.Qty, // will be update to baseqty on sp
                BaseUnit = item.UnitId // will be update to base unit on sp
            };
        }
        private void AddStockMutation(BeginningBalanceRequest data, BeginningBalanceStockDetail item) {
            if (item.Qty > 0)
            {
                var stockMutation = GetStockMutation(data, item);
                Db.StockMutations.Add(stockMutation);
            }
        }
      
        
    }
}
