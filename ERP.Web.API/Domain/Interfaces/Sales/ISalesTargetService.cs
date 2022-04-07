using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesTargetService : IGeneralService<SalesTargetHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search);

    IEnumerable<SalesTargetDetail> GetDetailData(string code);

    IEnumerable<SalesTargetSubject> GetSubjectData(string code);

    SaveResult Insert(SalesTargetRequest data);

    SaveResult Update(SalesTargetRequest data);

    SaveResult Delete(string code, int userId);

    SaveResult Clone(string code, string name, DateTime startDate, DateTime endDate, int userId);
}