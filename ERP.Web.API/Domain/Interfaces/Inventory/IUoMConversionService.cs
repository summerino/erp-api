using ERP.Entity.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory;

public interface IUoMConversionService
{
    IEnumerable<UoMConversion> GetData(int? uomId = null);
}