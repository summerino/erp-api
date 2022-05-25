using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Mobile.ItemRequest;
using ERP.Web.API.Domain.Models.Mobile.ItemRequest;

namespace ERP.Web.API.Domain.Services.Mobile.ItemRequest;

public class ItemRequestService : GeneralService<ItemRequestHeader>, IItemRequestService
{
    public ItemRequestService(TenantContext db) : base(db)
    {
    }
        
    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, int userId, int? areaId)
    {
        var data = (from header in Db.MobileItemRequestHeaders
            join user in Db.Users on header.SalesmanId equals user.EmployeeId
            join area1 in Db.Areas on header.AreaId1 equals area1.Id into a1
            from sub1 in a1.DefaultIfEmpty()
            join area2 in Db.Areas on header.AreaId2 equals area2.Id into a2
            from sub2 in a2.DefaultIfEmpty()
            join area3 in Db.Areas on header.AreaId3 equals area3.Id into a3
            from sub3 in a3.DefaultIfEmpty()
            join area4 in Db.Areas on header.AreaId4 equals area4.Id into a4
            from sub4 in a4.DefaultIfEmpty()
            join area5 in Db.Areas on header.AreaId5 equals area5.Id into a5
            from sub5 in a5.DefaultIfEmpty()
            where user.Id.Equals(userId) 
            select new ItemRequestHeader
            {
                Code = header.Code,
                Date = header.Date,
                TransferCode = header.TransferCode,
                AreaId1 = header.AreaId1,
                AreaId2 = header.AreaId2,
                AreaId3 = header.AreaId3,
                AreaId4 = header.AreaId4,
                AreaId5 = header.AreaId5,
                AreaName1 = sub1.Name,
                AreaName2 = sub2.Name,
                AreaName3 = sub3.Name,
                AreaName4 = sub4.Name,
                AreaName5 = sub5.Name,
                Mark = header.Mark
            }).AsQueryable();

        if (areaId.HasValue)
        {
            data = data.Where(x => x.AreaId1.Equals(areaId) || x.AreaId2.Equals(areaId) ||x.AreaId3.Equals(areaId) || x.AreaId4.Equals(areaId) || x.AreaId5.Equals(areaId));
        }

        data = data.OrderByDescending(x => x.Date).ThenByDescending(x => x.Code);

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public IEnumerable<ItemRequestDetail> GetDetail(string code)
    {
        var data = (from detail in Db.MobileItemRequestDetails
            join item in Db.Items on detail.ItemId equals item.Id
            join uom in Db.UoMConversions on detail.UnitId equals uom.Id into conv
            from sub in conv.DefaultIfEmpty()
            where detail.Code.Equals(code)
            select new ItemRequestDetail
            {
                ItemId=detail.ItemId,
                ItemName=item.Name,
                LineNo = detail.LineNo,
                UnitId=detail.UnitId,
                UnitName=sub.UnitEquivalent,
                Quantity=detail.Qty
            });

        return data;
    }

    public SaveResult Insert(ItemRequestModel data, int userId)
    {
        var result = new SaveResult(false);
        var empId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Get new code
            var newCode = GetNewCode("MOB_IR_NUM_FMT", data.Date, empId.ToString());

            // Insert header data
            data.Code = newCode;
            Db.MobileItemRequestHeaders.Add(data);

            // Insert detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                Db.MobileItemRequestDetails.Add(new MobileItemRequestDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    ItemId = item.ItemId,
                    UnitId = item.UnitId,
                    Qty = item.Qty
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
        result.Message = "Permintaan pembelian berhasil disimpan.";
        return result;
    }
}