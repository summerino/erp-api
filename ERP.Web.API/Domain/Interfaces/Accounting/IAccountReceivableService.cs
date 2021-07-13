using ERP.Web.API.Domain.Models;
using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IAccountReceivableService : IGeneralService<BeginningBalanceAR>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(int id, int userId);
    }
}
