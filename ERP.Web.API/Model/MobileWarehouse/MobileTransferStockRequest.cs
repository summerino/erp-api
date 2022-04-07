using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Model.MobileWarehouse;

public class MobileTransferStockRequest : MobileTransferStockHeader
{
    public IEnumerable<MobileTransferStockDetail> ItemDetails { get; set; }
}