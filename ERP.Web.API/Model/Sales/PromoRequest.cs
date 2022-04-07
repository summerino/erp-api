using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class PromoRequest : PromoHeader
{
    public IEnumerable<PromoDetailRequest> ItemDetails { get; set; }

    public IEnumerable<PromoSubjectRequest> SubjectDetails { get; set; }

    public DateTime? OriginalStartDate { get; set; }

    public DateTime? OriginalEndDate { get; set; }
}

public class PromoDetailRequest : PromoDetail
{
    public IEnumerable<PromoDetailTier> PromoTierList { get; set; }

    public IEnumerable<PromoDetailMultipleItem> MultipleItem { get; set; }

    public int? SaleUnit { get; set; }

    public bool ApplyToAllUnit { get; set; }

    public int? FreeGoodItemId { get; set; }

    public string UnitFreeGood { get; set; }

    public bool IsMultiple { get; set; }
}

public class PromoSubjectRequest : PromoSubject
{
    public string Subject { get; set; }
}