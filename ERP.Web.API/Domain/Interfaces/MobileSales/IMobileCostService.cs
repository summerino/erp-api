using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using ERP.Web.API.Model.MobileSales;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileCostService : IGeneralService<MobileCostHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwMobileCostDetail> GetDetailData(string code);

        SaveResult Approve(List<MobileCostRequest> data, int userId, string date, string coa, string notes);

        SaveResult Reject(List<MobileCostRequest> data, int userId);

        SaveResult Update(MobileCostRequest data);
    }
}
