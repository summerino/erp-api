using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Models;
using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface ICompanyProfileService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,  string search);

        SaveResult Update(Company data);
    }
}
