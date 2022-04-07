using ERP.Entity.Purchase;

namespace ERP.Web.API.Model.Purchase;

public class PurchaseReturnRequest : PurchaseReturnHeader
{
    public IEnumerable<PurchaseReturnDetail> ItemDetails { get; set; }

    public IEnumerable<PurchaseReturnDetailExchDiffItem> DiffItemDetails { get; set; }

    public DateTime? OriginalDate { get; set; }
}