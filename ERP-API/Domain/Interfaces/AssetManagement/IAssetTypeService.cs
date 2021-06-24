using System.Collections.Generic;
using ERP_API.Domain.Entities.AssetManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.AssetManagement
{
    public interface IAssetTypeService : IGeneralService<AssetType>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);

        DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

        SaveResult Delete(int id);
    }
}
