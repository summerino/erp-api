using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Finance;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance;

public interface IInterCashBankService : IGeneralService<GeneralCashBankHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);

    SaveResult Insert(CashBankRequest data);

    SaveResult Update(CashBankRequest data);

    SaveResult Delete(string code, string transCode, int userId);
}