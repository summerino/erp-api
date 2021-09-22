using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IIncomeStatementFormatService : IGeneralService<IncomeStatementFormat>
    {
        object GetFormatHierarchy(string category);

        IEnumerable<IncomeStatementFormat> GetFormatLists(string category);
    }
}
