using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.AssetManagement;

namespace ERP.Web.API.Domain.Interfaces.AssetManagement;

public interface IAssetTypeService : IGeneralService<AssetType>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search);

    DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

    SaveResult Delete(int id);
}