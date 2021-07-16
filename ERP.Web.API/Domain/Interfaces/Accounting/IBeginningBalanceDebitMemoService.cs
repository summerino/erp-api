using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBeginningBalanceDebitMemoService : IGeneralService<BeginningBalanceDebitMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(long id, int userId);
    }
}
