using ERP.Common;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Mobile.TransferStock;
using ERP.Web.API.Domain.Models.Mobile.TransferStock;

namespace ERP.Web.API.Domain.Services.Mobile.TransferStock;

public class MobileTransferStockService : GeneralService<MobileTransferStockHeader>, IMobileTransferStockService
{
    public MobileTransferStockService(TenantContext db) : base(db)
    {
    }

    public IEnumerable<MobileTransferStockHeaderModel> getTranferStockHeader(DateTime? date, string search, int userId)
    {
        var mobileTransferStock = (from mobilePO in Db.MobileTransferStockHeaders select mobilePO).ToList();
        var empId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var wh = Db.Employees.Where(x => x.Id.Equals(empId)).Select(y => y.WarehouseCode).Single();

        //var ap = Db.MobileTransferStockHeaders.Where(x => x.TransferCode == th.OriginTransferCode);
        var que = Db.MobileTransferStockHeaders.AsQueryable();

        var data = (from th in Db.VwTransferStockHeaders
                    join gi in Db.Warehouses on th.WarehouseCodeFrom equals gi.Code
                    join go in Db.Warehouses on th.WarehouseCodeTo equals go.Code
                    join emp1 in Db.Employees.Where(x => x.Type.Equals(2)) on th.WarehouseCodeFrom equals emp1.WarehouseCode
                        into a1
                    from sub1 in a1.DefaultIfEmpty()
                    join emp2 in Db.Employees.Where(x => x.Type.Equals(2)) on th.WarehouseCodeTo equals emp2.WarehouseCode
                        into a2
                    from sub2 in a2.DefaultIfEmpty()
                    where (th.WarehouseCodeTo == wh && th.Mark == "A")
                        || (th.WarehouseCodeFrom == wh && th.Type != "IN" && th.Mark == "A")
                        || (th.WarehouseCodeFrom == wh && th.Type == "IN" && (Db.MobileTransferStockHeaders.Where(x => x.Code.Equals(th.OriginTransferCode)).Select(y => y.Mark).Single()) == "A")
                        || (th.Type == "DT" && th.WarehouseCodeTo == wh && th.WarehouseCodeFrom != wh && th.Mark == "A")
                        || (th.Type == "DT" && th.WarehouseCodeTo != wh && th.WarehouseCodeFrom == wh && th.Mark == "A")
                    select new MobileTransferStockHeaderModel
                    {
                        Code = th.Code,
                        Date = th.Date,
                        Type = th.Type == "DT" ? "DT" : th.WarehouseCodeFrom == wh && th.WarehouseCodeTo != wh ? "OUT" : "IN",
                        TypeInitial = th.TypeInitial,
                        TransferCode = th.OriginTransferCode ?? "",
                        WarehouseCode = wh,
                        WarehouseCodeFrom = th.WarehouseCodeFrom,
                        WarehouseCodeTo = th.WarehouseCodeTo,
                        WarehouseInitialFrom = th.WarehouseInitialFrom,
                        WarehouseInitialTo = th.WarehouseInitialTo,
                        WarehouseNameFrom = gi.Name,
                        WarehouseNameTo = go.Name,
                        SalesNameFrom = sub1 != null ? sub1.FirstName + ' ' + sub1.LastName : "",
                        SalesNameTo = sub2 != null ? sub2.FirstName + ' ' + sub2.LastName : "",
                        Mark = th.Mark
                    }).AsQueryable();

        data = data.Where(x => !mobileTransferStock.Select(t => t.TransferCode).Contains(x.Code));

        if (date.HasValue)
        {
            data = data.Where(x => x.Date.Equals(date));
        }

        if (search != null && search != "")
        {
            data = data.Where(x => x.WarehouseNameFrom.Contains(search) || x.WarehouseNameTo.Contains(search) || x.Code.Contains(search));
        }

        data = data.OrderByDescending(x => x.Date);

        return data;
    }

    public IEnumerable<MobileTransferStockDetailModel> getTransferStockDetail(string code)
    {
        var data = from td in Db.VwTransferStockDetails
                   where td.Code == code
                   select new MobileTransferStockDetailModel
                   {
                       ItemId = td.ItemId,
                       UomId = td.UomId,
                       UnitId = td.UnitId,
                       ItemName = td.ItemName,
                       UnitName = td.UnitName,
                       OriginalQty = td.Qty,
                       RealizeQty = td.Qty
                   };

        return data;
    }

    public IEnumerable<MobileTransferStockHeaderModel> getMobileTranferStockHeader(DateTime? date, string search, int userId)
    {
        var empId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
        var wh = Db.Employees.Where(x => x.Id.Equals(empId)).Select(y => y.WarehouseCode).Single();

        var data = from thm in Db.MobileTransferStockHeaders
                   join th in Db.VwTransferStockHeaders on thm.TransferCode equals th.Code
                   join gi in Db.Warehouses on th.WarehouseCodeFrom equals gi.Code
                   join go in Db.Warehouses on th.WarehouseCodeTo equals go.Code
                   join emp1 in Db.Employees.Where(x => x.Type.Equals(2)) on th.WarehouseCodeFrom equals emp1.WarehouseCode
                       into a1
                   from sub1 in a1.DefaultIfEmpty()
                   join emp2 in Db.Employees.Where(x => x.Type.Equals(2)) on th.WarehouseCodeTo equals emp2.WarehouseCode
                       into a2
                   from sub2 in a2.DefaultIfEmpty()
                   where th.Mark == "A"
                         && ((th.WarehouseCodeTo == wh)
                             || (th.WarehouseCodeFrom == wh)
                             || (th.Type == "DT" && th.WarehouseCodeTo == wh && th.WarehouseCodeFrom != wh)
                             || (th.Type == "DT" && th.WarehouseCodeTo != wh && th.WarehouseCodeFrom == wh))
                   select new MobileTransferStockHeaderModel
                   {
                       Code = thm.Code,
                       Date = thm.CreatedDate.Date,
                       Type = th.Type == "DT" ? "DT" : th.WarehouseCodeFrom == wh && th.WarehouseCodeTo != wh ? "OUT" : "IN",
                       TypeInitial = th.TypeInitial,
                       TransferCode = thm.TransferCode ?? "",
                       WarehouseCode = wh,
                       WarehouseCodeFrom = th.WarehouseCodeFrom,
                       WarehouseCodeTo = th.WarehouseCodeTo,
                       WarehouseInitialFrom = th.WarehouseInitialFrom,
                       WarehouseInitialTo = th.WarehouseInitialTo,
                       WarehouseNameFrom = gi.Name,
                       WarehouseNameTo = go.Name,
                       SalesNameFrom = sub1 != null ? sub1.FirstName + ' ' + sub1.LastName : "",
                       SalesNameTo = sub2 != null ? sub2.FirstName + ' ' + sub2.LastName : ""
                   };

        if (date.HasValue)
        {
            data = data.Where(x => x.Date.Equals(date));
        }

        if (search != null && search != "")
        {
            data = data.Where(x => x.WarehouseNameFrom.Contains(search) || x.WarehouseNameTo.Contains(search) || x.Code.Contains(search));
        }

        data = data.OrderByDescending(x => x.Date).ThenByDescending(x => x.Code);

        return data;
    }

    public IEnumerable<MobileTransferStockDetailModel> getMobileTransferStockDetail(string code)
    {
        var data = from td in Db.VwMobileTransferStockDetails
                   where td.Code == code
                   select new MobileTransferStockDetailModel
                   {
                       ItemId = td.ItemId,
                       UomId = td.UomId,
                       UnitId = td.UnitId,
                       ItemName = td.ItemName,
                       UnitName = td.UnitName,
                       OriginalQty = td.OriginalQty,
                       RealizeQty = td.RealizeQty
                   };

        return data;
    }

    public SaveResult Insert(TransferStockRequestModel data, int userId)
    {
        var result = new SaveResult(false);
        var empId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();

        var existed_transfer_stock_code = Db.MobileTransferStockHeaders.Any(x => x.TransferCode == data.Code);
        if (!existed_transfer_stock_code)
        {
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var date = DateTime.Now;
                // Get new code
                var newCode = GetNewCode("MOB_TS_NUM_FMT", date, empId.ToString());

                // Insert header data
                Db.MobileTransferStockHeaders.Add(new MobileTransferStockHeader
                {
                    Code = newCode,
                    TransferCode = data.Code,
                    SignatureImage = data.SignatureImage,
                    Mark = "A",
                    CreatedBy = userId,
                    CreatedDate = date,
                    UpdatedBy = userId,
                    UpdatedDate = date,
                });

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.MobileTransferStockDetails.Add(new MobileTransferStockDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UnitId = item.UnitId,
                        UomId = item.UomId,
                        OriginalQty = item.OriginalQty,
                        RealizeQty = item.RealizeQty
                    });
                }

                // Save changes
                Db.SaveChanges();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Transfer Stok berhasil disimpan.";
            return result;
        }
        else
        {
            result.Success = false;
            result.Message = "Transfer Stok sudah pernah disimpan";
            return result;
        }
    }
}