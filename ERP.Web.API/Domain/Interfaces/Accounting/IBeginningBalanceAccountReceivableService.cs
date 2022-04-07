using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting;

public interface IBeginningBalanceAccountReceivableService : IGeneralService<BeginningBalanceAR>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    SaveResult Delete(long id, int userId);

    IEnumerable<UploadBBARRequest> VerifyUpload(IEnumerable<UploadBBARRequest> data);

    SaveResult Posting(IEnumerable<UploadBBARRequest> data, int userId);
}