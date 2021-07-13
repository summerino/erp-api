using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.AssetManagement;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.AssetManagement
{
    public interface IFixedAssetService : IGeneralService<FixedAsset>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(string code, int userId);
    }
}
