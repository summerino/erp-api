using ERP.Entity.Inventory;

namespace ERP.Web.API.Model.Inventory;

public class UnitOfMeasurementRequest : UoM
{
    public IEnumerable<UoMConversionRequest> Details { get; set; }
}

public class UoMConversionRequest
{
    public long Id { get; set; }

    public int UomId { get; set; }

    public string UnitToConvert { get; set; }

    public string UnitEquivalent { get; set; }

    public decimal Conversion { get; set; }

    public bool IsBaseUnit { get; set; }

    public int Seq { get; set; }
}