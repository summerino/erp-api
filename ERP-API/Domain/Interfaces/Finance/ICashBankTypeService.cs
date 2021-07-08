using ERP_API.Domain.Entities.Finance;
using ERP_API.Domain.Models;
using System.Linq;

namespace ERP_API.Domain.Interfaces.Finance
{
    public interface ICashBankTypeService
    {
        IQueryable<CashBankType> GetList();
        
    }
}
