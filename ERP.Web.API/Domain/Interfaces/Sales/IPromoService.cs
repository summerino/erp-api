using ERP.Entity.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IPromoService : IGeneralService<PromoHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
          string search);

        IEnumerable<PromoDetail> GetDetailData(string code);

        IEnumerable<PromoDetailTier> GetDetailTierData();

        SaveResult Insert(PromoRequest data);

        SaveResult Update(PromoRequest data);

        SaveResult Delete(string code, int userId);
    }
}
