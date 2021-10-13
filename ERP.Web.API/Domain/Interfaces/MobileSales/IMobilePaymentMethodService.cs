using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobilePaymentMethodService : IGeneralService<MobilePaymentMethod>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(int id, int userId);
    }
}
