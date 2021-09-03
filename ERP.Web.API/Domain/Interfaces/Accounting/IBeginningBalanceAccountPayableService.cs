using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBeginningBalanceAccountPayableService : IGeneralService<BeginningBalanceAP>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(long id, int userId);

        IEnumerable<UploadBBAPRequest> VerifyUpload(IEnumerable<UploadBBAPRequest> data);

        SaveResult Posting(IEnumerable<UploadBBAPRequest> data, int userId);
    }
}
