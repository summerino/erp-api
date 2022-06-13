using ERP.Entity.Accounting;

namespace ERP.Web.API.Model.Accounting
{
    public class IncomeStatementFormatRequest : IncomeStatementFormat
    {
        public IEnumerable<Coa> Coas { get; set; }
    }
}
