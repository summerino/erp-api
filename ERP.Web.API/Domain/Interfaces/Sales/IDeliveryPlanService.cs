using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IDeliveryPlanService : IGeneralService<DeliveryPlanHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<DeliveryPlanDetail> GetDetailData(string code);

        IEnumerable<DeliveryPlanUndeliveredItem> GetUndeliveredData();

        List<dynamic> GetRelatedTransactions(string code);

        DataSourceResult GetAllTransaction(string warehouseCode, IEnumerable<Filter> filter);


        SaveResult Insert(DeliveryPlanRequest data);

        SaveResult Update(DeliveryPlanRequest data);

        SaveResult Delete(string code, int userId);
    }
}
