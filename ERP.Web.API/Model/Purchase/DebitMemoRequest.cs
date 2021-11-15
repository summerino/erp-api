using ERP.Entity.Purchase;

namespace ERP.Web.API.Model.Purchase
{
    public class DebitMemoRequest : DebitMemo
    {
        public DateTime? OriginalDate { get; set; }
    }
}
