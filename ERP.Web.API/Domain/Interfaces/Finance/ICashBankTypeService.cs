using System.Collections.Generic;
using System.Linq;
using ERP.Entity.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankTypeService
    {
        IQueryable<VwCashBankType> GetLists(List<int> actionId);
    }
}
