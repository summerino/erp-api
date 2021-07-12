using ERP.Entity.Inventory;
using System.Collections.Generic;

namespace ERP.Web.API.Model.Inventory
{

    public class UnitOfMeasurementRequest : UoM
    {
        public IEnumerable<UoMConversion> Details { get; set; }
    }
}
