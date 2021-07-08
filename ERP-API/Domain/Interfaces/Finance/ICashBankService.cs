using ERP_API.Domain.Models;
using ERP_API.Model.Finance;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.Finance
{
    public interface ICashBankService : IGeneralService<CashBankRequest>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        SaveResult Insert(CashBankRequest data);
        SaveResult Update(CashBankRequest data);
        SaveResult Delete(string code, int userId);
    }
}
