using ERP.Web.API.Domain.Models;
using System.Linq;
using ERP.Entity.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface ICashBankTypeService
    {
        IQueryable<CashBankType> GetList();
        
    }
}
