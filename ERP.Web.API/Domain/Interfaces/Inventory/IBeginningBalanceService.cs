using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface IBeginningBalanceStockService : IGeneralService<BeginningBalanceStockHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        
        IEnumerable<VwBeginningBalanceStockDetail> GetDetailData(string code);
        
        SaveResult Insert(BeginningBalanceRequest data);
        
        SaveResult Update(BeginningBalanceRequest data);
        
        SaveResult Delete(string code, int userId);
    }
}
