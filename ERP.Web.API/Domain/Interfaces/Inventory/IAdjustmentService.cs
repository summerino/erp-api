using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory;

public interface IAdjustmentService : IGeneralService<AdjustmentHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    DataSourceResult GetAdjustmentItem(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        List<int> category, string search);

    IEnumerable<VwAdjustmentDetail> GetDetailData(string code);
    IEnumerable<AdjustmentDetailDiffUnit> GetDetailDiffUnit(string code);

    SaveResult Insert(AdjustmentRequest data);

    SaveResult Update(AdjustmentRequest data);

    SaveResult Delete(string code, int userId);
}