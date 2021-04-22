using ERP_API.Domain.Entities.Inventory;
using System.Collections.Generic;

namespace ERP_API.Model.Inventory
{

    public class UnitOfMeasurementRequest : UoM
    {
        public IEnumerable<UoMConversion> Details { get; set; }
    }
}
