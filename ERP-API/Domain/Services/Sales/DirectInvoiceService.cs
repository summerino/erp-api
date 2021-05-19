using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Services.Sales
{
    public class DirectInvoiceService : GeneralService<SalesInvoiceHeader>, IDirectInvoiceService
    {
        public DirectInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            throw new NotImplementedException();
        }

        public IEnumerable GetDetailData(string code)
        {
            throw new NotImplementedException();
        }

        public SaveResult Insert(SalesInvoiceRequest data)
        {
            throw new NotImplementedException();
        }

        public SaveResult Update(SalesInvoiceRequest data)
        {
            throw new NotImplementedException();
        }

        public SaveResult Delete(string code, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
