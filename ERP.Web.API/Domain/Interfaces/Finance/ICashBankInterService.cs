using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Finance;
using ERP.Web.API.Model.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankInterService : IGeneralService<GeneralCashBankHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwGeneralCashBankDetail> GetDetailData(string code);

        SaveResult Insert(CashBankInterRequest data);

        SaveResult Update(CashBankInterRequest data);

        SaveResult Delete(string code, int userId);
    }
}
