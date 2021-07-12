using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Models;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface ICompanyProfileService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,  string search);

        SaveResult Update(Company data);
    }
}
