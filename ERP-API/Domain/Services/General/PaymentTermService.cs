using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Domain.Services.General
{
    public class PaymentTermService : IPaymentTermService
    {
        private readonly TenantContext _tenantCtx;

        public PaymentTermService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = _tenantCtx.VwPaymentTerms.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }
    }
}
