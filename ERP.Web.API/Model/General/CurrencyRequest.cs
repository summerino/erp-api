using ERP.Entity.General;

namespace ERP.Web.API.Model.General;

public class CurrencyRequest : Currency
{
    public int SortValue { get; set; }
}