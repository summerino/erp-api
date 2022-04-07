using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.Warehouse;
using ERP.Web.API.Domain.Models.Mobile.Warehouse;

namespace ERP.Web.API.Domain.Services.Mobile.Warehouse;

public class WarehouseProfileService : IWarehouseProfileService
{
    private readonly TenantContext _db;
    public WarehouseProfileService(TenantContext db)
    {
        _db = db;
    }

    public WarehouseProfile GetWarehouseProfileForMobile(int id)
    {
        var data = (from user in _db.Users
            join empl in _db.Employees on user.EmployeeId equals empl.Id
            select new WarehouseProfile
            {
                UserId = user.Id,
                WarehouseEmployeeName = empl.FirstName + " " + empl.LastName,
                WarehouseEmployeeId = empl.Id,
                WarehouseEmployeeInitial = empl.Initial,
                WarehouseCode = empl.WarehouseCode
            }).SingleOrDefault(x => x.UserId.Equals(id));

        return data;
    }
}