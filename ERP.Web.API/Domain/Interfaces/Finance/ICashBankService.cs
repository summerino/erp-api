using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Finance;
using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankService : IGeneralService<CashBankRequest>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataDebitMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        DataSourceResult GetDataCreditMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);
        SaveResult Delete(string code, int userId);
    }
}
