using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Finance;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankService : IGeneralService<GeneralCashBankHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        DataSourceResult GetDataAR(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cbCode);

        DataSourceResult GetDataAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cbCode);

        DataSourceResult GetDataEPAP(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cbCode);

        DataSourceResult GetDataCreditMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cbCode, string type);

        DataSourceResult GetDataDebitMemo(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search, string cbCode, string type);

        IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);

        SaveResult Insert(CashBankRequest data);

        SaveResult Update(CashBankRequest data);

        SaveResult Delete(string code, int userId, int menuId, int roleId);
    }
}
