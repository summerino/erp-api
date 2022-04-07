using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Services.Inventory;

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
        
    public SaveResult Insert(BeginningBalanceRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var validationResult = Validate(data);
            if (!validationResult.Item1)
            {
                result.Message = validationResult.Item2;
                return result;
            }

            // Get new code
            var newCode = GetNewCode("BB_INVT_NUM_FMT", data.Date);

            // Insert header data
            data.Code = newCode;
            data.IsActive = true;
            Db.BeginningBalanceStockHeaders.Add(data);

            // Insert detail data
            short i = 0;
            var details = new List<BeginningBalanceStockDetail>();
            foreach (var item in data.ItemDetails)
            {
                var beginningBalanceDetail = new BeginningBalanceStockDetail
                {
                    LineNo = ++i,
                    Code = newCode,
                    ItemId = item.ItemId,
                    Notes = item.Notes,
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice,
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
            var validationResult = Validate(data);
            if (!validationResult.Item1)
            {
                result.Message = validationResult.Item2;
                return result;
            }

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
                    var beginningBalanceDetail = new BeginningBalanceStockDetail
                    {
                        Code = data.Code,
                        ItemId = item.ItemId,
                        LineNo = ++i,
                        Notes = item.Notes,
                        Qty = item.Qty,
                        UnitPrice = item.UnitPrice,
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
            var validationResult = ValidateDelete(code);
            if (!validationResult.Item1)
            {
                result.Message = validationResult.Item2;
                return result;
            }

            var data = Db.BeginningBalanceStockHeaders.Find(code);
            if (data != null)
            {
                //var stockBB = Db.StockMutations.Where(x => x.RefCode1 == data.Code);
                //if (stockBB != null)
                //{
                //    Db.StockMutations.RemoveRange(stockBB);
                //}

                // Execute sp_update_stock_mutation_from_bb
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_restore_stock_mutation_from_bb {0}, {1}",
                    data.Code, data.Date);

                var detail = Db.BeginningBalanceStockDetails.Where(x => x.Code.Equals(code)).ToList();
                Db.RemoveRange(detail);
                Db.Remove(data);

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
        result.Message = "Data saldo awal berhasil dihapus.";
        return result;
    }

    private (bool, string) ValidateDelete(string code) 
    {
        var listItem = Db.BeginningBalanceStockDetails.Where(x=>x.Code.Equals(code)).Select(x=>x.ItemId).ToList();

        if (Db.SalesOrderDetails.Any(x => listItem.Contains(x.ItemId)))
        {
            var items = (from d in Db.SalesOrderDetails
                join i in Db.Items on d.ItemId equals i.Id
                where listItem.Contains(i.Id)
                select i.Initial).Distinct().ToList();
            if (items.Count > 0)
            {
                var temp = string.Join(", ",items);
                string msg = $"Data {temp} sudah digunakan ditransaksi, saldo awal tidak bisa dihapus.";
                return (false, msg);
            }
        }

        return (true, "");
    }

    private (bool, string) Validate(BeginningBalanceRequest data)
    {
        IEnumerable<dynamic> listItemForSpesificWarehouse;
        var query = (from h in Db.BeginningBalanceStockHeaders
            join d in Db.BeginningBalanceStockDetails on h.Code equals d.Code
            join i in Db.Items on d.ItemId equals i.Id
            join u in Db.UoMConversions on d.UnitId equals u.Id
            where h.WarehouseCode == data.WarehouseCode
            select new
            {
                Code = d.Code,
                ItemID = d.ItemId,
                ItemInitial = i.Initial,
                UnitID = d.UnitId,
                UnitName = u.UnitEquivalent
            }).AsQueryable();
        if (data.Code == null)
        {
            //validate insert
            listItemForSpesificWarehouse = query.ToList();
        }
        else
        {
            //validate update
            listItemForSpesificWarehouse = query.Where(x=>x.Code != data.Code).ToList();
        }

        if (listItemForSpesificWarehouse.Any())
        {
            string message = "";
            List<string> items = new List<string>();
            foreach (var detail in data.ItemDetails)
            {
                var temp = listItemForSpesificWarehouse.FirstOrDefault(x => x.ItemID.Equals(detail.ItemId) && x.UnitID.Equals(detail.UnitId));
                if (temp != null)
                    items.Add($"{temp.ItemInitial} - [{temp.UnitName}]");
            }
            if (items.Count > 0)
            {
                string temp = string.Join(", ", items);
                message = $"Data {temp} sudah ada ditransaksi sebelumnya.";
                return (false, message);
            }
        }
        return (true, "");
    }

    public IEnumerable<UploadBBStockDetailRequest> VerifyUpload(IEnumerable<UploadBBStockDetailRequest> data)
    {
        foreach (var item in data)
        {
            var checkDupe = data.Where(x => x.Inisialbarang == item.Inisialbarang && x.Satuanbarang == item.Satuanbarang).Count() > 1;
            if (item.Qtybarang > 0 && item.Hargabarang > 0 && !checkDupe)
            {
                var itemData = Db.Items.FirstOrDefault(x => x.Initial == item.Inisialbarang);
                if (itemData != null)
                {
                    var uomData = Db.UoMConversions.Where(x => x.UomId == itemData.UomId).ToList();
                    if (!uomData.Select(x => x.UnitEquivalent).Contains(item.Satuanbarang))
                    {
                        item.Mark = true;
                    }
                }
                else
                {
                    item.Mark = true;
                }
            }
            else
            {
                item.Mark = true;
            }
        }

        return data;
    }

    public SaveResult Posting(UploadBBStockHeaderRequest data, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Get new code
            var newCode = GetNewCode("BB_INVT_NUM_FMT", data.Date);

            var headData = new BeginningBalanceStockHeader
            {
                Code = newCode,
                Date = data.Date,
                WarehouseCode = data.WarehouseCode,
                Notes = data.Notes,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                UpdatedBy = userId,
                UpdatedDate = DateTime.Now
            };
                

            // Insert detail data
            short i = 0;
            var details = new List<BeginningBalanceStockDetail>();
            foreach (var item in data.ItemDetails)
            {
                if (!item.Mark)
                {
                    var itemData = Db.Items.FirstOrDefault(x => x.Initial == item.Inisialbarang);
                    var uomData = Db.UoMConversions.FirstOrDefault(x => x.UomId == itemData.UomId &&
                                                                        x.UnitEquivalent == item.Satuanbarang);
                    var beginningBalanceDetail = new BeginningBalanceStockDetail
                    {
                        LineNo = ++i,
                        Code = newCode,
                        ItemId = itemData.Id,
                        Notes = item.Catatan,
                        Qty = item.Qtybarang,
                        UnitPrice = item.Hargabarang,
                        UnitId = uomData.Id,
                        UomId = uomData.UomId
                    };
                    details.Add(beginningBalanceDetail);
                }
            }

            if (!details.Any())
                return new SaveResult(false, "Tidak ada data saldo awal persediaan yang diproses.");

            var requestData = new BeginningBalanceRequest
            {
                Code = headData.Code,
                Date = headData.Date,
                WarehouseCode = headData.WarehouseCode,
                Notes = headData.Notes,
                IsActive = headData.IsActive,
                CreatedBy = headData.CreatedBy,
                CreatedDate = headData.CreatedDate,
                UpdatedBy = headData.UpdatedBy,
                UpdatedDate = headData.UpdatedDate,
                ItemDetails = details
            };

            var validationResult = Validate(requestData);
            if (!validationResult.Item1)
            {
                result.Message = validationResult.Item2;
                return result;
            }

            Db.BeginningBalanceStockHeaders.Add(headData);
            Db.BeginningBalanceStockDetails.AddRange(details);

            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_bb
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_bb {0}, {1}",
                newCode, data.Date);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Message = "Import data saldo awal stock berhasil disimpan.";
        return result;
    }
}