using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales
{
    public class CreditMemoRequest : CreditMemo
    {
        public DateTime? OriginalDate { get; set; }
    }
}
