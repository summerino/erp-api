using ERP.Entity.Inventory;

namespace ERP.Web.API.Model.Inventory
{
    public class UnitOfMeasurementRequest : UoM
    {
        public IEnumerable<UoMConversion> Details { get; set; }
    }
}
