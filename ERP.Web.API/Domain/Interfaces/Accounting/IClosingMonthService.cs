using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IClosingMonthService : IGeneralService<ClosingMonth>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        
        SaveResult Insert(ClosingMonthRequest data);

        bool IsMonthClosed(List<string> periods);
    }
}
