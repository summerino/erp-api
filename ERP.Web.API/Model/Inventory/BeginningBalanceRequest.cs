using ERP.Entity.Inventory;

namespace ERP.Web.API.Model.Inventory;

public class BeginningBalanceRequest : BeginningBalanceStockHeader
{
    public IEnumerable<BeginningBalanceStockDetail> ItemDetails { get; set; }

    public DateTime? OriginalDate { get; set; }
}