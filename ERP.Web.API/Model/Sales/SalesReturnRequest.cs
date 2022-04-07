using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class SalesReturnRequest : SalesReturnHeader
{
    public IEnumerable<SalesReturnDetail> ItemDetails { get; set; }

    public IEnumerable<SalesReturnDetailExchDiffItem> DiffItemDetails { get; set; }

    public DateTime? OriginalDate { get; set; }

}