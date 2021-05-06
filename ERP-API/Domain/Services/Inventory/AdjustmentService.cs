using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Services.Inventory
{
    public class AdjustmentService : GeneralService<AdjustmentHeader>, IAdjustmentService
    {
        public AdjustmentService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwAdjustmentHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.WarehouseInitial.Contains(search) || x.Notes.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwAdjustmentDetail> GetDetailData(string code)
        {
            var data = Db.VwAdjustmentDetails.Where(x => x.Code == code);
            return data.OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(AdjustmentRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("ADJ_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.AdjustmentHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    var adjustmentDetail = new AdjustmentDetail() {
                        LineNo = ++i,
                        BaseQtyOnHand = item.BaseQtyOnHand,
                        Code = newCode,
                        COGS = item.COGS,
                        ItemId = item.ItemId,
                        Notes = item.Notes,
                        QtyAdjust = item.QtyAdjust,
                        QtyOnHand = item.QtyOnHand,
                        UnitId = item.UnitId,
                        UomId = item.UomId
                    };

                    Db.AdjustmentDetails.Add(adjustmentDetail);
                    
                    // klo stock opname hanya isi yang berubah, klo multiple adjustment semua record di isi
                    if (item.QtyAdjust > 0) {
                        Db.StockMutations.Add(GetStockMutation(data, item, adjustmentDetail.Id));
                    } 
                }

                Db.SaveChanges();

                //Execute sp_update_stock_mutation_from_adj
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_adj {0}, {1}",
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
            result.Message = "Success insert adjustment.";
            return result;
        }

        public SaveResult Update(AdjustmentRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.AdjustmentHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Can't update adjutment because data already mark as void.";
                    return result;
                }

                // Update header data
                Db.AdjustmentHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                //Get detail data that exists in order before
                var delDetails = Db.AdjustmentDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.AdjustmentDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        var adjustmentDetail = new AdjustmentDetail()
                        {
                            BaseQtyOnHand = item.BaseQtyOnHand,
                            Code = data.Code,
                            COGS = item.COGS,
                            ItemId = item.ItemId,
                            LineNo = ++i,
                            Notes = item.Notes,
                            QtyAdjust = item.QtyAdjust,
                            QtyOnHand = item.QtyOnHand,
                            UnitId = item.UnitId,
                            UomId = item.UomId
                        };
                        Db.AdjustmentDetails.Add(adjustmentDetail);                        
                    }
                    else
                    {
                        item.LineNo = ++i;
                        Db.AdjustmentDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_adj
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_adj {0}, {1}",
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
            result.Message = "Success update adjustment.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.AdjustmentHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Can't void adjustment because data already mark as void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success void adjustment.";
            return result;
        }

        private StockMutation GetStockMutation(AdjustmentRequest data, AdjustmentDetail item, long adjustmentDetailId = 0) 
        {
            return new StockMutation
            {
                WarehouseCode = data.WarehouseCode,
                Date = data.Date,
                ItemId = item.ItemId,
                Qty = item.QtyAdjust,
                RefCode1 = data.Code,
                RefDetailId1 = adjustmentDetailId == 0 ? item.Id: adjustmentDetailId,
                Src = "ADJ",
                RefCode2 = null,
                UnitId = item.UnitId,
                UomId = item.UomId,
            };
        }
    }
}
