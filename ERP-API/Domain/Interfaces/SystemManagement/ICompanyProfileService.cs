using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;
using System.Collections.Generic;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface ICompanyProfileService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,  string search);

        SaveResult Update(Company data);
    }
}
