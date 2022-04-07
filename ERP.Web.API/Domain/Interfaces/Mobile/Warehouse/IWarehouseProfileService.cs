using ERP.Web.API.Domain.Models.Mobile.Warehouse;

namespace ERP.Web.API.Domain.Interfaces.Mobile.Warehouse;

public interface IWarehouseProfileService
{
    WarehouseProfile GetWarehouseProfileForMobile(int id);
}