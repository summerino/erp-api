using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Accounting;

public class BalanceSheetReportService : IBalanceSheetReportService
{
    private readonly TenantContext _db;

    public BalanceSheetReportService(TenantContext db)
    {
        _db = db;
    }
    public IEnumerable<BalanceSheetResult> GetBalanceSheetLists(IEnumerable<GeneralLedgerResult> data)
    {
        var result = new List<BalanceSheetResult>();
        var coaData = _db.Coas.FromSqlRaw(@"select *from Accounting.COA 
                            where Code Like '1%' 
                            or Code Like '2%' 
                            or Code Like '3%' 
                            or Code Like '4%'").ToList();

        foreach (var item in coaData)
        {
            var isParent = coaData.Where(x => x.ParentId == item.Id).ToList();
            if (isParent.Any())
            {
                var coaGroup = item.Code.Split('0')[0];
                var filterData = data.Where(x => x.CoaCode.StartsWith(coaGroup) && x.EndBalOc != null & x.CreditOc != null && x.DebetOc != null).ToList();

                result.Add(new BalanceSheetResult
                {
                    Code = item.Code,
                    Name = item.Name,
                    Amount = (decimal)filterData.Sum(x => x.EndBalOc),
                    Deep = item.Deep,
                    IsBold = true
                });
            }
            else
            {
                var filterData = data.Where(x => x.CoaCode == item.Code && x.EndBalOc != null & x.CreditOc != null && x.DebetOc != null).ToList();

                result.Add(new BalanceSheetResult
                {
                    Code = item.Code,
                    Name = item.Name,
                    Amount = (decimal)filterData.Sum(x => x.EndBalOc),
                    Deep = item.Deep,
                    IsBold = false
                });
            }
        }

        return result.OrderBy(x => x.Code);
    }
}