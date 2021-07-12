using ERP.Entity.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Interfaces.Sales
{
    public interface IDeliveryPlanService : IGeneralService<DeliveryPlanHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<DeliveryPlanDetail> GetDetailData(string code);

        IEnumerable<DeliveryPlanUndeliveredItem> GetUndeliveredData();

        List<dynamic> GetRelatedTransactions(string code);

        List<dynamic> GetAllTransaction(string warehousecode);

        SaveResult Insert(DeliveryPlanRequest data);

        SaveResult Update(DeliveryPlanRequest data);

        SaveResult Delete(string code, int userId);
    }
}
