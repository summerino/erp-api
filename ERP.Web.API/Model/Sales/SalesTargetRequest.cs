using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class SalesTargetRequest : SalesTargetHeader
{
    public IEnumerable<SalesTargetDetail> ItemDetails { get; set; }

    public IEnumerable<SalesTargetSubject> ItemSubjects { get; set; }
}