using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface IPromoService : IGeneralService<PromoHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search);

    IEnumerable<PromoDetail> GetDetailData(string code);

    IEnumerable<PromoDetailMultipleItem> GetMultipleItemsData();

    IEnumerable<PromoDetailTier> GetDetailTierData();

    IEnumerable<PromoSubject> GetSubjectData(string code);

    IEnumerable<object> GetListPromo(string code);

    SaveResult Insert(PromoRequest data);

    SaveResult Update(PromoRequest data);

    SaveResult Delete(string code, int userId);
}