using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IUoMConversionService
    {
        IEnumerable<UoMConversion> GetData(int? uomId = null);
    }
}
