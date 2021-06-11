using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Interfaces.General
{
    public interface IPaymentTermService
    {
        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

    }
}
