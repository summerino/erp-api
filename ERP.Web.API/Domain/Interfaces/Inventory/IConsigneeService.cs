using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Model.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface IConsigneeService : IGeneralService<TransferStockHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwTransferStockDetail> GetDetailData(string code);

        SaveResult Insert(TransferStockRequest data);

        SaveResult Update(TransferStockRequest data);

        SaveResult Delete(string code, int userId);
    }
}
