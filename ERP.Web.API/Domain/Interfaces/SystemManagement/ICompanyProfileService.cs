using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface ICompanyProfileService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        SaveResult Update(Company data);
    }
}
