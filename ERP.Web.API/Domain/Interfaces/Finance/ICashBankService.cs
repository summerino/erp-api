using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Finance;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankService : IGeneralService<CashBankRequest>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cashbankCode);

        DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cashbankCode);

        DataSourceResult GetDataDebitMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cashbankCode, string type);
        DataSourceResult GetDataCreditMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cashbankCode, string type);
        IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);

        SaveResult Delete(string code, int userId);
    }
}
