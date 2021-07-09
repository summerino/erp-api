using ERP_API.Domain.Entities.Finance;
using ERP_API.Domain.Models;
using ERP_API.Model.Finance;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.Finance
{
    public interface ICashBankService : IGeneralService<CashBankRequest>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);
        SaveResult Delete(string code, int userId);
    }
}
