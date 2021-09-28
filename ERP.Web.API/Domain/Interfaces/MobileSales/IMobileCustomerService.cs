using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileCustomerService : IGeneralService<MobileCustomer>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Approve(List<MobileCustomer> data, int userId);

        SaveResult Reject(List<MobileCustomer> data, int userId);

    }
}
