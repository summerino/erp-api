using ERP_API.Domain.Models;
using System.Linq;
using ERP.Entity.Finance;

namespace ERP_API.Domain.Interfaces.Finance
{
    public interface ICashBankTypeService
    {
        IQueryable<CashBankType> GetList();
        
    }
}
